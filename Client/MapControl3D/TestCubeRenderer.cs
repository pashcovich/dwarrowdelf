﻿using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;

namespace Dwarrowdelf.Client
{
	sealed class TestCubeRenderer : GameComponent
	{
		GeometricPrimitive m_cube;
		Texture2D m_cubeTexture;
		Matrix m_cubeTransform;

		BasicEffect m_basicEffect;

		public TestCubeRenderer(MyGame game)
			: base(game)
		{
			LoadContent();
		}

		void LoadContent()
		{
			m_basicEffect = ToDispose(new BasicEffect(this.GraphicsDevice));

			m_basicEffect.EnableDefaultLighting(); // enable default lightning, useful for quick prototyping
			m_basicEffect.TextureEnabled = true;   // enable texture drawing

			LoadCube();
		}

		public override void Update()
		{
			var time = (float)this.Game.Time.TotalTime.TotalSeconds;

			m_cubeTransform = Matrix.RotationX(time) * Matrix.RotationY(time * 2f) * Matrix.RotationZ(time * .7f);
		}

		public override void Draw(Camera camera)
		{
			m_basicEffect.View = Matrix.Translation(0, 0, 10);
			m_basicEffect.Projection = camera.Projection;

			m_basicEffect.Texture = m_cubeTexture;
			m_basicEffect.World = m_cubeTransform;
			m_cube.Draw(m_basicEffect);
		}

		void LoadCube()
		{
			m_cube = ToDispose(GeometricPrimitive.Cube.New(this.GraphicsDevice, 1, toLeftHanded: true));

			m_cubeTexture = this.Content.Load<Texture2D>("logo_large");

			m_cubeTransform = Matrix.Identity;
		}
	}
}
