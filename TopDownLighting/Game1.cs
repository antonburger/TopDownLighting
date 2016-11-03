using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TopDownLighting
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Map map;
        Effect effect;
        Texture2D floor;
        Texture2D wall;
        Texture2D box;
        Matrix view;
        Matrix proj;
        Light[] lights;
        LittleBox[] boxes;
        VertexBuffer boxVertices;
        IndexBuffer boxIndices;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            floor = Content.Load<Texture2D>("tex/floor");
            wall = Content.Load<Texture2D>("tex/wall");
            box = Content.Load<Texture2D>("tex/box");

            effect = Content.Load<Effect>("Map");

            // TODO: use this.Content to load your game content here
            var md = new MapDescription(10, 10, new MapWorldSpaceDimensions(1f, 2f));
            md.SetFloor(6, 2);
            md.SetFloor(7, 2);
            md.SetFloor(2, 3);
            md.SetFloor(4, 3);
            md.SetFloor(5, 3);
            md.SetFloor(6, 3);
            md.SetFloor(1, 4);
            md.SetFloor(2, 4);
            md.SetFloor(3, 4);
            md.SetFloor(4, 4);
            md.SetFloor(5, 4);
            md.SetFloor(6, 4);
            md.SetFloor(8, 4);
            md.SetFloor(9, 4);
            md.SetFloor(2, 5);
            md.SetFloor(4, 5);
            md.SetFloor(5, 5);
            md.SetFloor(6, 5);
            md.SetFloor(7, 3);
            md.SetFloor(7, 5);
            md.SetFloor(8, 5);
            md.SetFloor(9, 5);
            md.SetFloor(3, 6);
            md.SetFloor(4, 6);
            md.SetFloor(6, 6);
            md.SetFloor(8, 6);
            md.SetFloor(9, 6);
            md.SetFloor(3, 7);
            md.SetFloor(4, 7);
            md.SetFloor(5, 7);
            md.SetFloor(6, 7);
            md.SetFloor(3, 8);
            md.SetFloor(4, 8);
            var builder = new MapBuilder();
            map = builder.BuildMap(md, GraphicsDevice, floor, wall);

            lights = new[]
            {
                new Light(GraphicsDevice, 512)
                {
                    WorldPosition = new Vector3(5f, 2f, 5f),
                    WorldDirection = new Vector3(1, -0.5f, 1),
                    SpotAngleDegrees = 45f,
                    SpotExponent = 10,
                    ConstantAttenuation = 1f,
                    LinearAttenuation = 0.0f,
                    QuadraticAttenuation = 0.1f,
                },
                new Light(GraphicsDevice, 512)
                {
                    WorldPosition = new Vector3(7.5f, 2f, 2f),
                    WorldDirection = new Vector3(-1, -2f, 1),
                    SpotAngleDegrees = 45f,
                    SpotExponent = 10,
                    ConstantAttenuation = 1f,
                    LinearAttenuation = 0.0f,
                    QuadraticAttenuation = 0.1f,
                },
                new Light(GraphicsDevice, 512)
                {
                    WorldPosition = new Vector3(3f, 2f, 8f),
                    WorldDirection = new Vector3(2, -1, -1),
                    SpotAngleDegrees = 90f,
                    SpotExponent = 1,
                    ConstantAttenuation = 1f,
                    LinearAttenuation = 0.0f,
                    QuadraticAttenuation = 0.1f,
                },
            };

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(view = Matrix.CreateLookAt(new Vector3(6f, 6, 7.5f), new Vector3(6f, 0, 5.5f), Vector3.Up));
            effect.Parameters["Projection"].SetValue(proj = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 3), GraphicsDevice.Viewport.AspectRatio, 0.1f, 50));

            LittleBox.SetBuffers(boxVertices = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), 24, BufferUsage.WriteOnly), boxIndices = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6 * 2 * 3, BufferUsage.WriteOnly));
            boxes = new[]
            {
                new LittleBox(new Vector3(9, 0.8f, 5.5f), 0.5f),
                new LittleBox(new Vector3(9.1f, 1.5f, 5.2f), 0.3f),
                new LittleBox(new Vector3(6.5f, 0.25f, 3.5f), 0.5f),
            };
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            var mouseState = Mouse.GetState();
            var translation = Matrix.CreateTranslation(0, 0, 0);
            var nearPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.X, mouseState.Y, 0), proj, view, translation);
            var farPoint = GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.X, mouseState.Y, 1), proj, view, translation);
            var ray = new Ray(nearPoint, farPoint - nearPoint);
            var intersection = map.Intersects(ray);
            if (intersection != null)
            {
                var mapPoint = ray.Position + intersection.Value * ray.Direction;
                lights[0].WorldDirection = mapPoint - lights[0].WorldPosition;
                lights[0].WorldDirection.Normalize();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            effect.Parameters["shadowMap"].SetValue((Texture2D)null);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                DepthClipEnable = false,
            };
            effect.CurrentTechnique = effect.Techniques["SpotShadow"];
            foreach (var light in lights)
            {
                GraphicsDevice.SetRenderTarget(light.ShadowMap);
                GraphicsDevice.Clear(Color.Black);

                effect.Parameters["LightView"].SetValue(Matrix.CreateLookAt(light.WorldPosition, light.WorldPosition + light.WorldDirection, Vector3.Up));
                effect.Parameters["LightProjection"].SetValue(Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(light.SpotAngleDegrees), 1f, 1f, 100f));

                effect.Parameters["World"].SetValue(Matrix.Identity);
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    map.Draw(GraphicsDevice);
                }

                foreach (var box in boxes)
                {
                    effect.Parameters["World"].SetValue(box.CreateWorldMatrix());
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        box.Draw(GraphicsDevice);
                    }
                }
            }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                DepthClipEnable = true,
            };
            GraphicsDevice.Clear(Color.FromNonPremultiplied(new Vector4(new Vector3(0.1f), 1)));
            effect.CurrentTechnique = effect.Techniques["Ambient"];

            effect.Parameters["World"].SetValue(Matrix.Identity);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                map.Draw(GraphicsDevice);
            }

            effect.Parameters["diffuse"].SetValue(box);
            foreach (var box in boxes)
            {
                effect.Parameters["World"].SetValue(box.CreateWorldMatrix());
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    box.Draw(GraphicsDevice);
                }
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            effect.CurrentTechnique = effect.Techniques["Spot"];
            foreach (var light in lights)
            {
                effect.Parameters["shadowMap"].SetValue(light.ShadowMap);
                effect.Parameters["LightView"].SetValue(Matrix.CreateLookAt(light.WorldPosition, light.WorldPosition + light.WorldDirection, Vector3.Up));
                effect.Parameters["LightProjection"].SetValue(Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(light.SpotAngleDegrees), 1f, 1f, 100f));
                effect.Parameters["LightWorldPosition"].SetValue(light.WorldPosition);
                effect.Parameters["LightWorldDirection"].SetValue(light.WorldDirection);
                effect.Parameters["LightSpotCutoffCos"].SetValue((float)Math.Cos(MathHelper.ToRadians(light.SpotAngleDegrees / 2)));
                effect.Parameters["LightSpotExponent"].SetValue(light.SpotExponent);
                effect.Parameters["LightConstantAttenuation"].SetValue(light.ConstantAttenuation);
                effect.Parameters["LightLinearAttenuation"].SetValue(light.LinearAttenuation);
                effect.Parameters["LightQuadraticAttenuation"].SetValue(light.QuadraticAttenuation);

                effect.Parameters["World"].SetValue(Matrix.Identity);
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    map.Draw(GraphicsDevice);
                }

                effect.Parameters["diffuse"].SetValue(box);
                foreach (var box in boxes)
                {
                    effect.Parameters["World"].SetValue(box.CreateWorldMatrix());
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        box.Draw(GraphicsDevice);
                    }
                }
            }

            //spriteBatch.Begin(blendState: BlendState.Opaque);
            //spriteBatch.Draw(light.ShadowMap, Vector2.Zero, scale: null, color: Color.White);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
