using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Dungeon;

public class CaveGame : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private SpriteFont font;
    private Texture2D square;
    private KeyboardState oldState;

    private int mapSize = 50;
    private int[,] grid;
    private Vector2 player;

    public CaveGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        graphics.PreferredBackBufferWidth = mapSize * 20;
        graphics.PreferredBackBufferHeight = mapSize * 20;
        graphics.ApplyChanges();

        square = new Texture2D(GraphicsDevice, 1, 1);
        square.SetData(new[] { Color.White });

        grid = GenCave(250);
        grid = SmoothCave();

        player = new Vector2(mapSize / 2, mapSize / 2);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        font = Content.Load<SpriteFont>("fonts/Counter");
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
            
        if (state.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Left))
            if (grid[(int)player.Y, (int)player.X-1] == 1)
                player.X--;
        if (state.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Right))
            if (grid[(int)player.Y, (int)player.X+1] == 1)
                player.X++;
        if (state.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up))
            if (grid[(int)player.Y-1, (int)player.X] == 1)
                player.Y--;
        if (state.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down))
            if (grid[(int)player.Y+1, (int)player.X] == 1)
                player.Y++;


        oldState = state;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkSlateGray);

        // TODO: Add your drawing code here
        spriteBatch.Begin();

            // Draw grid
            for(int y = 0; y < mapSize; y++)
                for(int x = 0; x < mapSize; x++)
                    spriteBatch.Draw(square, new Rectangle(x*20+1, y*20+1, 18, 18), grid[y, x] == 1? Color.Yellow: Color.Black);

            spriteBatch.Draw(square, new Rectangle((int)player.X*20+1, (int)player.Y*20+1, 18, 18), Color.Red);

            spriteBatch.DrawString(font, $"examp", new Vector2(2, 0), Color.White);

        spriteBatch.End();

        base.Draw(gameTime);
    }

    public int[,] GenCave(int dungeonSize)
    {
        int counter = 1;
        int[,] arr = new int[mapSize, mapSize];
        arr[mapSize / 2, mapSize / 2] = 1;

        Walker[] walkers = new Walker[5];


        for(int i = 0; i < walkers.Length; i++)
            walkers[i] = new Walker(new Vector2(mapSize / 2, mapSize / 2));


        while(true)
            for(int i = 0; i < walkers.Length; i++)
            {
                if (counter >= dungeonSize)
                    return arr;
                walkers[i].Walk(arr, ref counter, dungeonSize, mapSize);
            }
    }

    public int[,] SmoothCave()
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
        for(int y = 0; y < mapSize; y++)
            for(int x = 0; x < mapSize; x++)
            {
                int neighbours = 0;
                foreach(int[] offset in offsets)
                {
                    if ((x == 0 && offset[0] == -1) || (x == mapSize-1 && offset[0] == 1)
                    ||(y == 0 && offset[1] == -1) || (y == mapSize-1 && offset[1] == 1))
                        continue;

                    if (grid[y + offset[1], x + offset[0]] == 1)
                        neighbours++;
                }
                
                if (neighbours > 4)
                    arr[y, x] = 1;
                else arr[y, x] = 0;
            }

        return arr;
    }
}
