using System;
using Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Walker
{
    public Vector2 Pos;
    private Random rnd;

    public Walker(Vector2 pos)
    {
        Pos = pos;
        rnd = new Random();
    }

    public void Walk(int[,] grid, ref int counter, int dungeonSize, int mapSize)
    {
        if (counter >= dungeonSize)
            return;

        int dir = rnd.Next(4);
        switch(dir)
        {
            case 0:
                if (Pos.X == 0)
                    Pos.X++;
                else Pos.X--;
                break;
            case 1:
                if (Pos.X == mapSize-1)
                    Pos.X--;
                else Pos.X++;
                break;
            case 2:
                if (Pos.Y == 0)
                    Pos.Y++;
                else Pos.Y--;
                break;
            case 3:
                if (Pos.Y == mapSize-1)
                    Pos.Y--;
                else Pos.Y++;
                break;
        }

        if (grid[(int)Pos.Y, (int)Pos.X] != 0)
            counter++;

        grid[(int)Pos.Y, (int)Pos.X] = 0;
    }
}

public class AnimatedSprite
{
    public Texture2D Texture { get; set; }
    public int Rows { get; set; }
    public int Columns { get; set; }
    private int currFrame;
    private int sumFrames;

    public AnimatedSprite(Texture2D texture, int rows, int columns)
    {
        Texture = texture;
        Rows = rows;
        Columns = columns;
        currFrame = 0;
        sumFrames = rows * columns;
    }

    public void Update()
    {
        currFrame++;
        if (currFrame == sumFrames)
            currFrame = 0;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        int width = Texture.Width / Columns;
        int height = Texture.Height / Rows;
        int row = currFrame / Columns;
        int column = currFrame % Columns;

        Rectangle source = new Rectangle(width * column, height * row, width, height);
        Rectangle dest = new Rectangle((int)pos.X, (int)pos.Y, width, height);

        spriteBatch.Draw(Texture, dest, source, Color.White);
    }
}

public class Camera
{
    public Matrix Transform { get; private set; }
    public float Zoom { get; set; }

    public Camera(float zoom)
    {
        Zoom = zoom;
    }

    public void Follow(Vector2 obj)
    {
        Matrix pos = Matrix.CreateTranslation(
          - obj.X * CaveGame.gameScale - 8,
          - obj.Y * CaveGame.gameScale - 8,
            0
        );

        Matrix offset = Matrix.CreateTranslation(
            CaveGame.width / 2 / Zoom,
            CaveGame.height / 2 / Zoom,
            0
        );

        Transform = pos * offset * Matrix.CreateScale(Zoom);
    }
}

public class Player
{
    private static int[,] grid;
    public Vector2 pos;

    public Player(Vector2 pos, int[,] grid)
    {
        Player.grid = grid;
        this.pos = pos;
    }
}