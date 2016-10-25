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
            GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
            };

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
            md.SetFloor(4, 4);
            md.SetFloor(4, 5);
            md.SetFloor(4, 6);
            md.SetFloor(5, 5);
            md.SetFloor(6, 4);
            md.SetFloor(6, 5);
            md.SetFloor(6, 6);
            //md.SetFloor(1, 0);
            //md.SetFloor(5, 5);
            var builder = new MapBuilder();
            map = builder.BuildMap(md, GraphicsDevice, floor, wall);

            effect.Parameters["World"].SetValue(world = Matrix.Identity);
            effect.Parameters["View"].SetValue(view = Matrix.CreateLookAt(new Vector3(5.5f, 5, 6.5f), new Vector3(5.5f, 0, 4.5f), new Vector3(0.3f, 0, -0.8f)));
            effect.Parameters["Projection"].SetValue(proj = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 3), GraphicsDevice.Viewport.AspectRatio, 0.1f, 50));
            effect.Parameters["LightWorldPosition"].SetValue(new Vector3(4.5f, 1.5f, 5.5f));
            effect.Parameters["LightWorldDirection"].SetValue(new Vector3(1, -0.5f, 1));
            effect.Parameters["LightTightness"].SetValue(12.0f);
            effect.Parameters["LightBrightness"].SetValue(5.0f);
            effect.CurrentTechnique = effect.Techniques["PerPixelConeLight"];
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
                var newLightDirection = mapPoint - effect.Parameters["LightWorldPosition"].GetValueVector3();
                newLightDirection.Normalize();
                effect.Parameters["LightWorldDirection"].SetValue(newLightDirection);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                map.Draw(GraphicsDevice);
            }

            base.Draw(gameTime);
        }
    }
}
