using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Dungeon;

public class CaveGame : Game
{
    public static int gameScale = 16;
    public static int width;
    public static int height;

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private SpriteFont font;


    private Camera camera;
    private Random rnd;
    private Texture2D square;
    private Texture2D atlas;
    private Dictionary<int, Rectangle> sources;
    private KeyboardState oldState;

    private int mapSize = 50;
    private int[,] grid;
    private int[,] baked;
    private List<Vector2> lights;
    private Player player;


    public CaveGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        width = mapSize * gameScale;
        height = mapSize * gameScale;

        // TODO: Add your initialization logic here
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        graphics.ApplyChanges();

        camera = new Camera(4f);

        rnd = new Random();

        square = new Texture2D(GraphicsDevice, 1, 1);
        square.SetData(new[] { Color.White });

        sources = new Dictionary<int, Rectangle>()
        {
            { 0, new Rectangle(16, 0, 8, 8) },
            { 1, new Rectangle(0, 0, 8, 8) },
            { 2, new Rectangle(0, 8, 8, 8) },
            { 3, new Rectangle(8, 8, 8, 8) },
            { 4, new Rectangle(16, 8, 8, 8) },
            { 5, new Rectangle(24, 8, 8, 8) }
        };

        grid = GenCave(250);
        lights = new List<Vector2>();

        player = new Player(new Vector2(mapSize / 2, mapSize / 2), grid);

        BakeLights();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        font = Content.Load<SpriteFont>("fonts/Counter");
        atlas = Content.Load<Texture2D>("sprites/stone pack");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        KeyboardState state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.Space) && !oldState.IsKeyDown(Keys.Space))
            grid = SmoothCave();

        if (state.IsKeyDown(Keys.R) && !oldState.IsKeyDown(Keys.R))
            grid = GenCave(400);

        // Player movement
        if (state.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.A))
            if (player.pos.X - 1 > 0 && grid[(int)player.pos.Y, (int)player.pos.X - 1] == 0)
                player.pos.X--;
        if (state.IsKeyDown(Keys.D) && !oldState.IsKeyDown(Keys.D))
            if (player.pos.X + 1 < mapSize && grid[(int)player.pos.Y, (int)player.pos.X + 1] == 0)
                player.pos.X++;
        if (state.IsKeyDown(Keys.W) && !oldState.IsKeyDown(Keys.W))
            if (player.pos.Y - 1 > 0 && grid[(int)player.pos.Y - 1, (int)player.pos.X] == 0)
                player.pos.Y--;
        if (state.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.S))
            if (player.pos.Y + 1 < mapSize && grid[(int)player.pos.Y + 1, (int)player.pos.X] == 0)
                player.pos.Y++;

        // Mine
        if (state.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Left))
            if (player.pos.X - 1 > 0)
                grid[(int)player.pos.Y, (int)player.pos.X - 1] = 0;

        if (state.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Right))
            if (player.pos.X < mapSize - 1)
                grid[(int)player.pos.Y, (int)player.pos.X + 1] = 0;

        if (state.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up))
            if (player.pos.Y > 0)
                grid[(int)player.pos.Y - 1, (int)player.pos.X] = 0;

        if (state.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down))
            if (player.pos.Y < mapSize - 1)
                grid[(int)player.pos.Y + 1, (int)player.pos.X] = 0;


        if (state.GetPressedKeyCount() != 0)
            BakeLights();

        oldState = state;

        camera.Follow(player.pos);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: camera.Transform
        );

        // Draw grid
        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
                spriteBatch.Draw(atlas, new Rectangle(x * gameScale, y * gameScale, gameScale, gameScale), sources[grid[y, x]], Color.White * ((float)baked[y, x] / 8));

        // Draw player
        spriteBatch.Draw(square, new Rectangle((int)player.pos.X * gameScale, (int)player.pos.Y * gameScale, gameScale, gameScale), Color.White);

        spriteBatch.End();

        // Draw UI
        spriteBatch.Begin();

        spriteBatch.DrawString(font, $"examp", new Vector2(2, 0), Color.White);

        spriteBatch.End();

        base.Draw(gameTime);
    }

    protected int[,] GenCave(int dungeonSize)
    {
        int counter = 1;
        int[,] arr = new int[mapSize, mapSize];
        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
                arr[y, x] = rnd.Next(5) + 1;
        arr[mapSize / 2, mapSize / 2] = 0;
        Walker[] walkers = new Walker[5];


        for (int i = 0; i < walkers.Length; i++)
            walkers[i] = new Walker(new Vector2(mapSize / 2, mapSize / 2));


        while (true)
            for (int i = 0; i < walkers.Length; i++)
            {
                if (counter >= dungeonSize)
                    return arr;
                walkers[i].Walk(arr, ref counter, dungeonSize, mapSize);
            }
    }

    protected int[,] SmoothCave()
    {
        int[][] offsets = {
            new[]{-1,-1},
            new[]{0,-1},
            new[]{1,-1},
            new[]{-1,0},
            new[]{1,0},
            new[]{-1,1},
            new[]{0,1},
            new[]{1,1}
        };

        int[,] arr = new int[mapSize, mapSize];
        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
            {
                int neighbours = 0;
                foreach (int[] offset in offsets)
                {
                    if ((x == 0 && offset[0] == -1) || (x == mapSize - 1 && offset[0] == 1)
                    || (y == 0 && offset[1] == -1) || (y == mapSize - 1 && offset[1] == 1))
                        continue;

                    if (grid[y + offset[1], x + offset[0]] == 0)
                        neighbours++;
                }

                if (neighbours > 4)
                    arr[y, x] = 0;
                else arr[y, x] = rnd.Next(5) + 1;
            }

        return arr;
    }

    protected void BakeLights()
    {
        baked = new int[mapSize, mapSize];

        LightUp(player.pos, 5);

        foreach (Vector2 light in lights)
            LightUp(light, 8);

        /* for(int y = 0; y < mapSize; y++)
        {
            for(int x = 0; x < mapSize; x++)
                Console.Write(baked[y, x] + " ");
            Console.WriteLine();
        } */
    }

    protected void LightUp(Vector2 pos, int strength, Vector2? prev = null)
    {
        if (pos.X <= 0 || pos.X >= mapSize || pos.Y <= 0 || pos.Y >= mapSize)
            return;

        if (strength == 0 || baked[(int)pos.Y, (int)pos.X] >= strength)
            return;

        baked[(int)pos.Y, (int)pos.X] = strength;

        if (grid[(int)pos.Y, (int)pos.X] != 0)
            return;

        Vector2[] offsets = new Vector2[]
        {
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(0, 1),
            new Vector2(0, -1)
        };
        foreach (Vector2 offset in offsets)
            if (pos + offset != prev)
                LightUp(pos + offset, strength - 1, pos);
    }
}
