using System;
using System.Collections.Generic;
using Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Walker
{
    public Vector2 Pos;

    public Walker(Vector2 pos)
    {
        Pos = pos;
    }

    public void Walk(int[,] grid, ref int counter, int dungeonSize, int mapSize)
    {
        if (counter >= dungeonSize)
            return;

        int dir = CaveGame.rnd.Next(4);
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
    private Vector2 camPos;

    public Camera(float zoom, Vector2 startPos)
    {
        Zoom = zoom;
        camPos = startPos;
    }

    public void Follow(Vector2 obj) 
    {
        Vector2 lerped = Vector2.Lerp(
            camPos, new Vector2(obj.X * CaveGame.gameScale - 8, obj.Y * CaveGame.gameScale - 8), .05f
        );

        Matrix pos = Matrix.CreateTranslation(
            - lerped.X,
            - lerped.Y, 0
        );

        camPos = lerped;

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
    public Vector2 pos;
    public int itemIndex;
    private int dir = 0; // 0 - front | 1 - right | 2 - back | 3 - left

    public Player(Vector2 pos)
    {
        this.pos = pos;
        itemIndex = 0; // 0 - pickaxe | 1 - torch
    }

    public void Input(KeyboardState state, KeyboardState oldState)
    {
        // Movement
        if (state.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.A))
            if (pos.X - 1 > 0 && CaveGame.grid[(int)pos.Y, (int)pos.X - 1] == 0)
                {
                    pos.X--;
                    dir = 3;
                }
        if (state.IsKeyDown(Keys.D) && !oldState.IsKeyDown(Keys.D))
            if (pos.X + 1 < CaveGame.mapSize && CaveGame.grid[(int)pos.Y, (int)pos.X + 1] == 0)
                {
                    pos.X++;
                    dir = 1;
                }
        if (state.IsKeyDown(Keys.W) && !oldState.IsKeyDown(Keys.W))
            if (pos.Y - 1 > 0 && CaveGame.grid[(int)pos.Y - 1, (int)pos.X] == 0)
                {
                    pos.Y--;
                    dir = 2;
                }
        if (state.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.S))
            if (pos.Y + 1 < CaveGame.mapSize && CaveGame.grid[(int)pos.Y + 1, (int)pos.X] == 0)
                {
                    pos.Y++;
                    dir = 0;
                }

        // Interaction
        if (state.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Left))
            if (pos.X - 1 > 0)
                Interact((int)pos.Y, (int)pos.X - 1);

        if (state.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Right))
            if (pos.X < CaveGame.mapSize - 1)
                Interact((int)pos.Y, (int)pos.X + 1);

        if (state.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up))
            if (pos.Y > 0)
                Interact((int)pos.Y - 1, (int)pos.X);

        if (state.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down))
            if (pos.Y < CaveGame.mapSize - 1)
                Interact((int)pos.Y + 1, (int)pos.X);

        // Inventory
        if (state.IsKeyDown(Keys.D1) && !oldState.IsKeyDown(Keys.D1))
            itemIndex = 0;
        if (state.IsKeyDown(Keys.D2) && !oldState.IsKeyDown(Keys.D2))
            itemIndex = 1;
        if (state.IsKeyDown(Keys.D3) && !oldState.IsKeyDown(Keys.D3))
            itemIndex = 2;
    }

    public void Interact(int y, int x)
    {
        if (itemIndex == 0)
        {
            if (CaveGame.grid[y, x] != 0)
                CaveGame.grid[y, x] = 0;
            if (CaveGame.lights.Contains(new Vector2(x, y)))
                CaveGame.lights.Remove(new Vector2(x, y));
            return;
        }
        else if (CaveGame.grid[y, x] != 0 || CaveGame.lights.Contains(new Vector2(x, y)))
            return;

        switch(itemIndex)
        {
            case 1: // Torch
                CaveGame.lights.Add(new Vector2(x, y));
                break;

            case 2: // Wooden Box
                CaveGame.grid[y, x] = 7;
                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        spriteBatch.Draw(
            texture, new Rectangle((int)pos.X * CaveGame.gameScale, (int)pos.Y * CaveGame.gameScale, CaveGame.gameScale, CaveGame.gameScale),
            new Rectangle(dir * 8, 0, 8, 8), Color.White
        );
    }
}

public class Menu
{
    public Button[] buttons;
    private int index;

    public Menu(Button[] buttons)
    {
        this.buttons = buttons;
        index = 0;
    }

    public void Input(KeyboardState state, KeyboardState oldState)
    {
        if (state.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up))
            index--;
        if (state.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down))
            index++;
            
        index = Math.Clamp(index, 0, buttons.Length-1);
        
        if (state.IsKeyDown(Keys.Enter) && !oldState.IsKeyDown(Keys.Enter))
            buttons[index].fn();
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font, int yOffset)
    {
        for(int i = 0; i < buttons.Length; i++)
            spriteBatch.DrawString(font, buttons[i].name, new Vector2(CaveGame.width/2 - font.MeasureString(buttons[i].name).X/2, yOffset + i * 100), i == index? Color.Yellow: Color.White*.5f);
    }
}

public class Button
{
    public string name;
    public Action fn;

    public Button(string name, Action fn)
    {
        this.name = name;
        this.fn = fn;
    }
}