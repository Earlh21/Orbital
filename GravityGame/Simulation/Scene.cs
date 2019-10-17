using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
	public class Scene : Drawable
	{
		private readonly int THREAD_COUNT = Environment.ProcessorCount - 1;

		private List<Star> star_cache;
		private List<RenderBody> body_buffer;
		private List<Effect> effect_buffer;
		private Body selected;

		private List<Thread> forces_threads;
		private List<AutoResetEvent> forces_handles;
		private List<AutoResetEvent> main_handles;
		private Rectangle important_area;

		public bool DrawOutlines { get; set; }
		public QuadTree QuadTree { get; private set; }
		public bool DrawText { get; set; }
		public Body Selected => selected;

		public List<RenderBody> Bodies { get; set; }

		public List<Effect> Effects { get; set; }


		public Scene()
		{
			QuadTree = new QuadTree(WorldSize(), null, false);
			
			Bodies = new List<RenderBody>();
			Effects = new List<Effect>();
			
			star_cache = new List<Star>();
			body_buffer = new List<RenderBody>();
			effect_buffer = new List<Effect>();

			forces_threads = new List<Thread>();
			forces_handles = new List<AutoResetEvent>();
			main_handles = new List<AutoResetEvent>();
			for (int i = 0; i < THREAD_COUNT; i++)
			{
				int temp = i;
				Thread thread = new Thread(() => PartialForces(temp));
				forces_threads.Add(thread);
				forces_handles.Add(new AutoResetEvent(false));
				main_handles.Add(new AutoResetEvent(false));
			}

			foreach (Thread thread in forces_threads)
			{
				thread.Start();
			}
		}

		private void PartialForces(int thread_id)
		{
			while (true)
			{
				int length = Bodies.Count;

				int section_length = length / THREAD_COUNT + 1;

				int low = section_length * thread_id;
				int high = section_length * (thread_id + 1);

				for (int i = low; i < high && i < length; i++)
				{
					Body body = Bodies[i];
					if (body.Exists && !body.ForcesDone)
					{
						if (important_area.ContainsPoint(body.Position))
						{
							body.Force += body.GetForceFrom(QuadTree, 0.8f);
						}
						else
						{
							body.Force += body.GetForceFrom(QuadTree, 1.2f);
						}

						body.ForcesDone = true;
					}
				}

				main_handles[thread_id].Set();
				forces_handles[thread_id].WaitOne();
			}
		}

		public void AddBody(RenderBody body)
		{
			body_buffer.Add(body);
		}
		
		public void AddEffect(Effect effect)
		{
			effect_buffer.Add(effect);
		}

		public void EvolveLifeAtPosition(Vector2f position)
		{
			foreach (RenderBody body in Bodies)
			{
				if (body.Contains(position))
				{
					if (body is Planet planet)
					{
						planet.EvolveLife();
					}

					break;
				}
			}
		}

		public void ForceBodyBufferInsert()
		{
			AddBodies();
		}
		
		
		
		private void AddBodies()
		{
			foreach (RenderBody body in body_buffer)
			{
				Bodies.Add(body);
				if (body.DoesGravity)
				{
					QuadTree.Insert(body);
				}

				if (body is Star star)
				{
					star_cache.Add(star);
				}
			}

			body_buffer.Clear();
		}
		
		private void AddEffects()
		{
			Effects.AddRange(effect_buffer);
			effect_buffer.Clear();
		}

		public Vector2f GetTotalMomentum()
		{
			Vector2f total = new Vector2f(0, 0);
			foreach (Body body in Bodies)
			{
				total += body.Momentum;
			}

			return total;
		}

		public void RemoveBody(RenderBody body)
		{
			if (body.IsSelected)
			{
				selected = null;
			}

			Bodies.Remove(body);
			if (body.DoesGravity)
			{
				QuadTree.Remove(body);
			}

			if (body is Star)
			{
				star_cache.Remove((Star) body);
			}
		}

		public void Deselect()
		{
			if (selected != null)
			{
				selected.IsSelected = false;
				selected = null;
			}
		}

		public bool SelectAt(Vector2f position)
		{
			bool found = false;

			foreach (RenderBody body in Bodies)
			{
				if (body.Contains(position))
				{
					Select(body);

					found = true;
					break;
				}
			}

			return found;
		}

		public Vector2f GetSelectedPosition()
		{
			if (selected == null)
			{
				return new Vector2f(0, 0);
			}

			return selected.Position;
		}

		public Vector2f GetSelectedVelocity()
		{
			if (selected == null)
			{
				return new Vector2f(0, 0);
			}

			return selected.Velocity;
		}

		public void Select(Body body)
		{
			if (selected != null)
			{
				selected.IsSelected = false;
			}

			if (body.IsSelectable)
			{
				selected = body;
				body.IsSelected = true;
			}
		}

		public bool MakeStarAt(Vector2f position)
		{
			RenderBody b = null;
			bool found = false;

			foreach (RenderBody body in Bodies)
			{
				if (body.Contains(position) && !(body is Star))
				{
					body.Exists = false;
					Star star = new Star(body.Position, body.Velocity, body.Mass);
					AddBody(star);
					b = body;

					found = true;
					break;
				}
			}

			if (found)
			{
				RemoveBody(b);
			}

			return found;
		}

		public Rectangle WorldSize()
		{
			return new Rectangle(new Vector2f(-50000, -50000), new Vector2f(100000, 100000));
		}

		public Rectangle GetAABB()
		{
			float highest = 0;

			foreach (Body body in Bodies)
			{
				if (body.Position.X + body.Radius > highest)
				{
					highest = body.Position.X + body.Radius;
				}

				if (body.Position.X - body.Radius < -highest)
				{
					highest = -body.Position.X + body.Radius;
				}

				if (body.Position.Y + body.Radius > highest)
				{
					highest = body.Position.Y + body.Radius;
				}

				if (body.Position.Y - body.Radius < -highest)
				{
					highest = -body.Position.Y + body.Radius;
				}
			}

			Vector2f bottomleftbound = new Vector2f(-highest, -highest) * 1.1f;
			Vector2f size = bottomleftbound * -2;

			return new Rectangle(bottomleftbound, size);
		}

		private QuadTree GetQuadTree(bool iteration)
		{
			QuadTree tree = new QuadTree(WorldSize(), null, iteration);

			foreach (Body body in Bodies)
			{
				if (iteration)
				{
					if (!WorldSize().FullyContains(body.Position, body.Radius))
					{
						body.Exists = false;
						continue;
					}
				}
				else
				{
					if (!WorldSize().FullyContains(body))
					{
						body.Exists = false;
						continue;
					}
				}

				tree.Insert(body);
			}

			//Cache centers of mass
			tree.CalculateCenterOfMass();

			return tree;
		}

		public void Draw(RenderTarget target, RenderStates states)
		{
			RenderWindow window = (RenderWindow) target;
			View view = window.GetView();

			Vector2f real_center = new Vector2f(view.Center.X, -view.Center.Y);
			Vector2f bottom_left = real_center - view.Size / 2;
			Rectangle domain = new Rectangle(bottom_left, view.Size);

			foreach (Effect effect in Effects)
			{
				target.Draw(effect);
			}
			
			foreach (RenderBody body in Bodies)
			{
				if (body is Star)
				{
					continue;
				}
				
				if (domain.PartiallyContains(body) && body.Exists)
				{
					body.DrawOutline = DrawOutlines;
					if (body is IDrawsText)
					{
						((IDrawsText) body).DrawText = DrawText;
					}

					target.Draw(body);
				}
			}

			foreach (Star star in star_cache)
			{
				target.Draw(star);
			}
			
			Texture screen = new Texture(target.Size.X, target.Size.Y);
			screen.Update((RenderWindow)target);
			//screen.Update(new Image("C:\\Users\\Idrialite\\Pictures\\barcode_test.jpg"));
			
			Sprite blackhole = new Sprite(screen);
			blackhole.Position = new Vector2f(0, 0);
			blackhole.Scale = new Vector2f(1.0f / Program.ViewScale, 1.0f / Program.ViewScale);
			
			Shader blackhole_shader = new Shader(null, null, Program.GetResourcesDirectory() + "\\Shaders\\blackhole.frag");
			blackhole_shader.SetUniform("screen", screen);
			
			target.Draw(blackhole, new RenderStates(blackhole_shader));
		}

		private void Iterate(float time)
		{
			foreach (Body body in Bodies)
			{
				if (body.Exists)
				{
					body.Iterate(time);
					body.Started = true;
					body.ForcesDone = false;

					if (body.DoesGravity)
					{
						QuadTree.UpdateBody(body);
					}
				}
			}
		}

		private void ResolveCollisions()
		{
			//Find collisions
			List<CollisionPair> collisions = new List<CollisionPair>();

			foreach (Body body in Bodies)
			{
				QuadTree leaf;

				if (body.DoesGravity)
				{
					leaf = QuadTree.Search(body);
				}
				else
				{
					leaf = QuadTree.SearchPosition(body.Position);
				}

				collisions.AddRange(body.GetCollisions(body.GetSmallestContainingTree(leaf)));
			}

			//Resolve collisions
			foreach (CollisionPair collision in collisions)
			{
				Body primary = collision.Resolve(this);

				if (primary != null)
				{
					if (primary != collision.A)
					{
						QuadTree.Remove(collision.A);
					}
					else
					{
						QuadTree.Remove(collision.B);
					}
					
					QuadTree.UpdateBody(primary);
				}
			}
		}

		private void ApplyStarHeat(float time)
		{
			foreach (Body body in Bodies)
			{
				if (body is TemperatureBody)
				{
					TemperatureBody t_body = (TemperatureBody) body;
					foreach (Star star in star_cache)
					{
						t_body.Heat += t_body.GetHeatFlowFrom(star) * time;
					}
				}
			}
		}

		private void RemoveNonexistentBodies()
		{
			Rectangle domain = WorldSize();
			
			for (int i = Bodies.Count - 1; i > -1; i--)
			{
				if(!domain.FullyContains(Bodies[i]))
				{
					RemoveBody(Bodies[i]);
				}
				else if (!Bodies[i].Exists)
				{
					Bodies.RemoveAt(i);
				}
			}
		}

		private void RemoveEffects()
		{
			for (int i = Effects.Count - 1; i > -1; i--)
			{
				if (Effects[i].Remove)
				{
					Effects.RemoveAt(i);
				}
			}
		}

		public void UpdateBodies(float time)
		{
			foreach (Body body in Bodies)
			{
				if (body.Exists)
				{
					body.Update(this, time);
				}
			}
		}

		public void UpdateEffects(float time)
		{
			foreach (Effect effects in Effects)
			{
				effects.Update(time);
			}
		}

		//TODO: This crashes if it gets below 0
		public void LeechStarMass(float amount)
		{
			if (star_cache.Count > 0)
			{
				int index = Program.R.Next(star_cache.Count);

				star_cache[index].SubtractBasicMass(amount);
			}
		}

		public Star GetMainStar()
		{
			Star biggest = null;
			
			foreach (Star star in star_cache)
			{
				if (biggest == null || star.Mass > biggest.Mass)
				{
					biggest = star;
				}
			}

			return biggest;
		}
		
		public void Update(float time, Rectangle important_area)
		{
			this.important_area = important_area;
			
			AddBodies();
			QuadTree.CalculateCenterOfMass();
			
			foreach (AutoResetEvent forces_handle in forces_handles)
			{
				forces_handle.Set();
			}
			
			ApplyStarHeat(time);
			UpdateBodies(time);
			
			AddEffects();
			RemoveEffects();

			foreach (AutoResetEvent main_handle in main_handles)
			{
				main_handle.WaitOne();
			}
			
			UpdateEffects(time);
			
			Iterate(time);

			ResolveCollisions();
			RemoveNonexistentBodies();
		}
	}
}