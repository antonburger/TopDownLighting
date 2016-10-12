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
        BasicEffect effect;

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
                CullMode = CullMode.None
            };

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

            // TODO: use this.Content to load your game content here
            var md = new MapDescription(10, 10, new MapWorldSpaceDimensions(1f, 1f));
            md.SetFloor(4, 4);
            md.SetFloor(6, 5);
            //md.SetFloor(1, 0);
            //md.SetFloor(5, 5);
            var builder = new MapBuilder();
            map = builder.BuildMap(md, GraphicsDevice);

            effect = new BasicEffect(GraphicsDevice);
            effect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            effect.DiffuseColor = new Vector3(1, 0, 0);
            effect.LightingEnabled = true;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = new Vector3(1);
            effect.DirectionalLight0.Direction = new Vector3(1);
            effect.VertexColorEnabled = false;

            effect.World = Matrix.Identity;
            effect.View = Matrix.CreateLookAt(new Vector3(0, 5, 10), new Vector3(5, 0, 5), Vector3.Up);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 3), GraphicsDevice.Viewport.AspectRatio, 0.1f, 50);
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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                map.Draw(GraphicsDevice);
            }

            base.Draw(gameTime);
        }
    }
}
