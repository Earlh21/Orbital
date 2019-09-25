using SFML.Graphics;

namespace GravityGame
{
	public abstract class Effect : Drawable
	{
		public float KillTime { get; protected set; }
		public float LifeTime { get; private set; }
		public bool Remove => LifeTime > KillTime;

		public Effect()
		{
			
		}

		public virtual void Update(float time)
		{
			LifeTime += time;
		}

		public abstract void Draw(RenderTarget target, RenderStates states);
	}
}