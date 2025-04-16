using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Dungeon;

public class CaveGame : Game
{
    public static int gameState; // 0 - menu | 1 - game
    public static int width;
    public static int height;
    public static int gameScale = 16;
    public static int mapSize = 50;
    public static List<Vector2> lights;
    public static int[,] grid;
    public static int[,] baked;

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private SpriteFont fontMenu;
    private SpriteFont fontHUD;

    private Camera camera;
    private Random rnd;
    private Texture2D square;
    private Texture2D atlas;
    private Dictionary<int, Rectangle> sources;
    private KeyboardState oldState;

    private Menu mainMenu;

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

        sources = new Dictionary<int, Rectangle>()
        {
            { 0, new Rectangle(16, 0, 8, 8) },
            { 1, new Rectangle(0, 0, 8, 8) },
            { 2, new Rectangle(0, 8, 8, 8) },
            { 3, new Rectangle(8, 8, 8, 8) },
            { 4, new Rectangle(16, 8, 8, 8) },
            { 5, new Rectangle(24, 8, 8, 8) },
            { 6, new Rectangle(0, 16, 8, 8) }
        };

        grid = new int[mapSize, mapSize];

        camera = new Camera(4f);

        rnd = new Random();

        square = new Texture2D(GraphicsDevice, 1, 1);
        square.SetData(new[] { Color.White });

        mainMenu = new Menu(new Button[]
            {
                new Button("start", Start),
                new Button("quit", Exit)
            }
        );



        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        fontMenu = Content.Load<SpriteFont>("fonts/Menu");
        fontHUD = Content.Load<SpriteFont>("fonts/Hud");
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

        if (gameState == 1)
        {
            player.Input(state, oldState);
            camera.Follow(player.pos);
            
            if (state.GetPressedKeyCount() != 0)
               BakeLights();
        }
        else mainMenu.Input(state, oldState);

        oldState = state;


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        if (gameState == 1)
        {
            // TODO: Add your drawing code here
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: camera.Transform
            );

            // Draw grid
            for(int y = 0; y < mapSize; y++)
                for(int x = 0; x < mapSize; x++)
                    spriteBatch.Draw(atlas, new Rectangle(x * gameScale, y * gameScale, gameScale, gameScale), sources[grid[y, x]], Color.White * ((float)baked[y, x] / 8));

            foreach(Vector2 light in lights)
                spriteBatch.Draw(atlas, new Rectangle((int)light.X * gameScale, (int)light.Y * gameScale, gameScale, gameScale), sources[6], Color.White);

            // Draw player
            spriteBatch.Draw(square, new Rectangle((int)player.pos.X * gameScale, (int)player.pos.Y * gameScale, gameScale, gameScale), Color.White);

            spriteBatch.End();
        }

        // Draw UI
        spriteBatch.Begin();

        if (gameState == 1)
            spriteBatch.DrawString(fontHUD, "Inventory: " + new string[]{ "pickaxe", "torch" }[player.itemIndex], new Vector2(10, height-30), Color.White);
        else mainMenu.Draw(spriteBatch, fontMenu, 300);
            

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
        foreach(Vector2 offset in offsets)
            if (pos + offset != prev)
                LightUp(pos + offset, strength - 1, pos);
    }

    public void Start()
    {
        grid = GenCave(250);
        lights = new List<Vector2>();
        player = new Player(new Vector2(mapSize / 2, mapSize / 2));
        BakeLights();
        gameState = 1;
    }
}
