using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Animation2D;

namespace ELEVENS
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        int screenHeight;
        int screenWidth;

        int cardHeight;
        int cardWidth;

        List<int> CardsList = new List<int>();
        List<int>[,] board3D = new List<int> [2, 6];

        MouseState mouse;
        Vector2 MouseLoc;
        Vector2 mouseClickLoc;
        Vector2 prevMouseClickLoc;

        //Card Selection variables
        int selectedPile1;
        int selectedPile2;
        bool IsCard1Selected;
        bool IsCard2Selected;

        //Saves x and y coordinates of the card piles chosen
        int boardSpotSelected1;
        int boardSpotSelected2;

        //Dest Rec's for card piles
        //Rectangle[] pileRectangles = new Rectangle[12];
        List<Rectangle> pileRectangles = new List<Rectangle>();

        //Fonts
        SpriteFont TitleFont;

        //Images and their rectangles
        Texture2D bgImg;
        Rectangle bgRec;

        Texture2D cardsImg;
        Texture2D cardBackImg;
        Rectangle cardRec;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            screenWidth = 1200;
            screenHeight = 800;

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;

            graphics.ApplyChanges();

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
            TitleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
            bgImg = Content.Load<Texture2D>("images/backgrounds/table");
            cardsImg = Content.Load<Texture2D>("images/sprites/CardFaces");
            cardBackImg = Content.Load<Texture2D>("images/sprites/CardBack");

            bgRec = new Rectangle(0, 0, screenWidth, screenHeight);

            cardWidth = cardsImg.Width / 13;
            cardHeight = cardsImg.Height / 13;

            LoadBoardLists();
            CardInit();
            SetUpCardsFromDeck();
            DefinePileRectangles();

            // TODO: use this.Content to load your game content here
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

            UserMouseDetails();

           
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param
        protected override void Draw(GameTime gameTime)
        {
            this.IsMouseVisible = true;
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Draw(bgImg, bgRec, Color.White);

            /*
             * when writing the draw for the board, make sure that you are drawing the top card aka list.length - 1
             * because 0 will be the first card (bottom)
             */


            for (int boardColumn = 0; boardColumn < board3D.GetLength(1); ++boardColumn)
            {
                for (int boardRow = 0; boardRow < board3D.GetLength(0); ++boardRow)
                {
                    //Ugly
                    spriteBatch.DrawString(TitleFont, Convert.ToString(board3D[boardRow, boardColumn][board3D[boardRow, boardColumn].Count - 1]), new Vector2(200 * (boardColumn + 1), 200 * (boardRow + 1)), Color.White);

                }
                
            }

            
            spriteBatch.DrawString(TitleFont, Convert.ToString(MouseLoc), MouseLoc, Color.White);
            
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        //Subprogram that loads up the cards as integers into the CardsList list. 
        //The integer is later moduolized by 13 to determine which suit the card is
        public void CardInit()
        {
            for (int value = 0; value < 52; ++value)
            {
                CardsList.Add(value);
            }
        }
        
        public void DrawCardImages(int cardValue)
        {
            //spriteBatch.Draw(cardsImg, )
        }

        public void LoadBoardLists()
        {
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    board3D[boardrow,boardcolumn] = new List<int>();
                }
            }
        }

        public void SetUpCardsFromDeck()
        {
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    board3D[boardrow, boardcolumn].Add(CardsList[0]);
                    CardsList.RemoveAt(0);
                }
            }
        }

        
        public bool IsValidCardCombination(int Card1, int Card2)
        {
            
            int sumofSelectedCards = (Card1 + Card2) % 13;

            
            if (sumofSelectedCards == 10)
            {
                return true;
            }

            else
            {
                return false;
            }
            
        }
               
        //Redo the logic, have another subprogram handle the actual logic, this is just for the mouse location
        public void UserMouseDetails()
        {
            mouse = Mouse.GetState();
            MouseLoc.X = mouse.X;
            MouseLoc.Y = mouse.Y;
            
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                mouseClickLoc.X = mouse.X;
                mouseClickLoc.Y = mouse.Y;

                if (IsCard1Selected == false)
                {
                    selectedPile1 = ClickQuadrant(mouseClickLoc);

                    if (selectedPile1 > 0)
                    {
                        IsCard1Selected = true;
                    }
                }
                
                if (IsCard1Selected == true)
                {
                    selectedPile2 = ClickQuadrant(mouseClickLoc);

                    if (selectedPile2 > 0)
                    {
                        IsCard2Selected = true;

                        IsValidCardCombination(selectedPile1, selectedPile2);
                    }
                }

            }
        }

        //ASK LANE
        public int ClickQuadrant(Vector2 MouseXY)
        {
            for (int i = 0; i < pileRectangles.Count; ++i)
            {
                if (pileRectangles[i].Contains(MouseXY))
                {
                    return i;
                }
                
            }

            return -1;
        }
        
        public void DefinePileRectangles()
        {
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    pileRectangles.Add(new Rectangle((100 + cardWidth * boardcolumn), (100 + cardHeight * boardrow), cardWidth, cardHeight));
                }
            }
        }
    }
}
