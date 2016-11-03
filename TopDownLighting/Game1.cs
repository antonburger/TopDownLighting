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
        Matrix world;
        Matrix view;
        Matrix proj;
        Light light;

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

            light = new Light(GraphicsDevice, 512);
            light.WorldPosition = new Vector3(5f, 1f, 5f);
            light.WorldDirection = new Vector3(1, -0.5f, 1);
            light.SpotAngleDegrees = 30f;
            light.SpotExponent = 1;
            light.ConstantAttenuation = 1f;
            light.LinearAttenuation = 0.0f;
            light.QuadraticAttenuation = 0.1f;

            effect.Parameters["World"].SetValue(world = Matrix.Identity);
            effect.Parameters["View"].SetValue(view = Matrix.CreateLookAt(new Vector3(6f, 7, 6.5f), new Vector3(6f, 0, 5.5f), Vector3.Up));
            effect.Parameters["Projection"].SetValue(proj = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 3), GraphicsDevice.Viewport.AspectRatio, 0.1f, 50));
            effect.Parameters["LightWorldPosition"].SetValue(light.WorldPosition);
            effect.Parameters["LightWorldDirection"].SetValue(light.WorldDirection);
            effect.Parameters["LightSpotCutoffCos"].SetValue((float)Math.Cos(MathHelper.ToRadians(light.SpotAngleDegrees / 2)));
            effect.Parameters["LightSpotExponent"].SetValue(light.SpotExponent);
            effect.Parameters["LightConstantAttenuation"].SetValue(light.ConstantAttenuation);
            effect.Parameters["LightLinearAttenuation"].SetValue(light.LinearAttenuation);
            effect.Parameters["LightQuadraticAttenuation"].SetValue(light.QuadraticAttenuation);
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
                light.WorldDirection = mapPoint - effect.Parameters["LightWorldPosition"].GetValueVector3();
                light.WorldDirection.Normalize();
                effect.Parameters["LightWorldDirection"].SetValue(light.WorldDirection);
                effect.Parameters["LightView"].SetValue(Matrix.CreateLookAt(light.WorldPosition, light.WorldPosition + light.WorldDirection, Vector3.Up));
                effect.Parameters["LightProjection"].SetValue(Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(light.SpotAngleDegrees), 1f, 1f, 100f));
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
            GraphicsDevice.SetRenderTarget(light.ShadowMap);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
            };
            GraphicsDevice.Clear(Color.Black);
            effect.CurrentTechnique = effect.Techniques["SpotShadow"];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                map.Draw(GraphicsDevice);
            }

            GraphicsDevice.SetRenderTarget(null);
            effect.Parameters["shadowMap"].SetValue(light.ShadowMap);
            GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
            };
            GraphicsDevice.Clear(Color.FromNonPremultiplied(new Vector4(new Vector3(0.1f), 1)));
            effect.CurrentTechnique = effect.Techniques["Ambient"];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                map.Draw(GraphicsDevice);
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            effect.CurrentTechnique = effect.Techniques["Spot"];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                map.Draw(GraphicsDevice);
            }

            //spriteBatch.Begin(blendState: BlendState.Opaque);
            //spriteBatch.Draw(light.ShadowMap, Vector2.Zero, scale: null, color: Color.White);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
