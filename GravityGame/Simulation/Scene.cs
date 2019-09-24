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

		private List<RenderBody> bodies;
		private List<Star> star_cache;
		private List<RenderBody> body_buffer;
		private Body selected;

		private List<Thread> forces_threads;
		private List<AutoResetEvent> forces_handles;
		private List<AutoResetEvent> main_handles;
		private Rectangle important_area;

		public bool DrawOutlines { get; set; }
		public QuadTree QuadTree { get; private set; }
		public bool DrawText { get; set; }
		public Body Selected => selected;

		public List<RenderBody> Bodies => bodies;

		public Scene()
		{
			QuadTree = new QuadTree(WorldSize(), null, false);
			bodies = new List<RenderBody>();
			star_cache = new List<Star>();
			body_buffer = new List<RenderBody>();

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
				int length = bodies.Count;

				int section_length = length / THREAD_COUNT + 1;

				int low = section_length * thread_id;
				int high = section_length * (thread_id + 1);

				for (int i = low; i < high && i < length; i++)
				{
					Body body = bodies[i];
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

		public void EvolveLifeAtPosition(Vector2f position)
		{
			foreach (RenderBody body in bodies)
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
				bodies.Add(body);
				QuadTree.Insert(body);
				if (body is Star star)
				{
					star_cache.Add(star);
				}
			}

			body_buffer.Clear();
		}

		public Vector2f GetTotalMomentum()
		{
			Vector2f total = new Vector2f(0, 0);
			foreach (Body body in bodies)
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

			bodies.Remove(body);
			QuadTree.Remove(body);
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

			foreach (RenderBody body in bodies)
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

			foreach (RenderBody body in bodies)
			{
				if (body.Contains(position) && !(body is Star))
				{
					body.Exists = false;
					Star star = new Star(body.Position, body.Mass, body.Velocity, body.Density);
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

			foreach (Body body in bodies)
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

			foreach (Body body in bodies)
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

			foreach (RenderBody body in bodies)
			{
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
		}

		private void Iterate(float time)
		{
			foreach (Body body in bodies)
			{
				if (body.Exists)
				{
					body.Iterate(time);
					body.Started = true;
					body.ForcesDone = false;
					
					QuadTree.UpdateBody(body);
				}
			}
		}

		//TODO: Game crashes when objects go outside the simulation because they're not contained in the quad tree
		//Fix GetSmallestContainingTree or something, delete the objects before GetAllCollisions is called
		private void ResolveCollisions()
		{
			//Find collisions
			List<Pair> collisions = new List<Pair>();

			collisions.AddRange(Body.GetAllCollisions(QuadTree));

			//Resolve collisions
			foreach (Pair collision in collisions)
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
			foreach (Body body in bodies)
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
			
			for (int i = bodies.Count - 1; i > -1; i--)
			{
				if(!domain.FullyContains(bodies[i]))
				{
					RemoveBody(bodies[i]);
				}
				else if (!bodies[i].Exists)
				{
					bodies.RemoveAt(i);
				}
			}
		}

		public void UpdateBodies(float time)
		{
			foreach (Body body in bodies)
			{
				if (body.Exists)
				{
					body.Update(this, time);
				}
			}
		}

		//TODO: This crashes if it gets below 0
		public void LeechStarMass(float amount)
		{
			if (star_cache.Count > 0)
			{
				int index = Program.R.Next(star_cache.Count);

				star_cache[index].Mass -= amount;
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

			foreach (AutoResetEvent main_handle in main_handles)
			{
				main_handle.WaitOne();
			}
			
			Iterate(time);

			ResolveCollisions();
			RemoveNonexistentBodies();
		}
	}
}