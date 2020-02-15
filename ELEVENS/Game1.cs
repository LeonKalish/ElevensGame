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

        Random rng = new Random();

        int screenHeight;
        int screenWidth;

        int cardHeight;
        int cardWidth;

        //Make the cards slightly larger when being drawn
        double sizeModifier = 1.30;

        List<int> CardsList = new List<int>();
        List<int>[,] board3D = new List<int> [2, 6];

        MouseState mouse;
        Vector2 MouseLoc;
        Vector2 mouseClickLoc;
        Vector2 prevMouseClickLoc;

        //Card Selection variables
        Vector2 selectedPile1;
        Vector2 selectedPile2;
        bool IsCard1Selected;
        bool IsCard2Selected;

        //Saves x and y coordinates of the card piles chosen
        int boardSpotSelected1;
        int boardSpotSelected2;

        //Dest Rec's for card piles
        //Rectangle[] pileRectangles = new Rectangle[12];
        Rectangle[,] pileRectangles = new Rectangle [2,6];

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
            cardHeight = cardsImg.Height / 4;

            LoadBoardLists();
            CardInit();
            ShuffleCards();
            SetUpCardsFromDeck();
            DefineDestRecs();

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
                    int cardSuit = ((board3D[boardRow, boardColumn][board3D[boardRow, boardColumn].Count-1]) / 13); 
                    int cardRank = ((board3D[boardRow, boardColumn][board3D[boardRow, boardColumn].Count-1]) % 13);

                    
                    //spriteBatch.Draw(cardsImg, pileRectangles[boardRow,boardColumn][board3D[boardRow,boardColumn].Count-1], new Rectangle((cardWidth * boardColumn), cardHeight * boardColumn, cardWidth, cardHeight), Color.White);
                    spriteBatch.Draw(cardsImg, pileRectangles[boardRow, boardColumn], new Rectangle((cardWidth * cardRank), cardHeight * cardSuit, cardWidth, cardHeight), Color.White);

                    //spriteBatch.DrawString(TitleFont, Convert.ToString(board3D[boardRow, boardColumn][board3D[boardRow, boardColumn].Count - 1]), new Vector2((50*boardColumn) + 100, (100 * boardRow) + 100), Color.Red);
                   
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

        //public Vector2 DrawLogicForCards(int cardValue)
        //{
        //    int cardSuit = cardValue / 13;
        //    int cardRank = cardValue % 13;

        //    return new Vector2(cardSuit, cardRank);
        //}

        //Initializes the board's lists which act as piles where are cards are added to. 
        public void LoadBoardLists()
        {
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    board3D[boardrow,boardcolumn] = new List<int>();

                    //Is this right???
                    pileRectangles[boardrow, boardcolumn] = new Rectangle();
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


        public bool IsValidCardCombination(Vector2 pile1XY, Vector2 pile2XY)
        {
            
            //int sumofSelectedCards = ( (board3D[pile1XY.X,pile1XY.Y][board3D[[pile1XY.X, pile1XY.Y].Coun]- 1+ pile2XY) % 13;
            int sumOfSelectedCards = ((board3D[(int)pile1XY.X, (int)pile1XY.Y][board3D[(int)pile1XY.X, (int)pile1XY.Y].Count - 1]) + ((board3D[(int)pile2XY.X, (int)pile2XY.Y][board3D[(int)pile2XY.X, (int)pile2XY.Y].Count - 1])) % 13);
            
            if (sumOfSelectedCards == 10)
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

            }

            CardClickLogic(mouseClickLoc);
        }

        public void CardClickLogic(Vector2 mouseClickLoc)
        {
            if (IsCard1Selected == false)
            {
                selectedPile1 = ClickQuadrant(mouseClickLoc);

                if (selectedPile1!= new Vector2(-1, -1))
                {
                    IsCard1Selected = true;
                }
            }

            if (IsCard1Selected == true)
            {
                selectedPile2 = ClickQuadrant(mouseClickLoc);

                if (selectedPile2 != new Vector2(-1,-1))
                {
                    IsCard2Selected = true;

                   IsValidCardCombination(board3D[(int)selectedPile1.X, (int)selectedPile1.Y], board3D[(int)selectedPile2.X, (int)selectedPile2.Y]));
                }
            }

        }

        public void ShuffleCards()
        {
            for (int i = 0; i < Math.Pow(10,4); ++i)
            {
                for (int j = 0; j < CardsList.Count; ++j)
                {
                    int randomIndex = rng.Next(CardsList.Count);
                    int firstCard = CardsList[j];
                    int secondCard = CardsList[randomIndex];

                    CardsList[randomIndex] = firstCard;
                    CardsList[j] = secondCard;
                }
            }
                
        }

        //ASK LANE
        public Vector2 ClickQuadrant(Vector2 MouseXY)
        {
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    if (pileRectangles[boardrow,boardcolumn].Contains(MouseXY))
                    {
                        return new Vector2 (boardrow,boardcolumn);
                    }
                }
            }

            return new Vector2(-1, -1);
        }
        
        //This subprogram defines the rectangles to which the images will be drawn to. The number of the pile correlates to the pile itself (Pile 0 = top left destRec (0)).
        public void DefineDestRecs()
        {
            //Iterate through the rows then the columns, so the rectangles are initizalized by columns. 
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    //pileRectangles[boardrow,boardcolumn] = new Rectangle((100 + (cardWidth * boardcolumn) +50 ), (200 + (cardHeight * boardrow + 50)), cardWidth, cardHeight);
                    pileRectangles[boardrow, boardcolumn] = new Rectangle((boardcolumn * cardWidth*2) + 100, (boardrow * cardHeight * 2) + 200, Convert.ToInt32(cardWidth*sizeModifier), Convert.ToInt32(cardHeight*sizeModifier));
                }
            }
        }
    }
}
