// Author: Leon Kalish
// File Name: Main.cs
// Project Name: ELEVENS
// Creation Date: January 31 2020 
// Modified Date: Feb. 16, 2020
// Description: This game is made to the simulate the solitarie variant: Elevens.

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
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Random variable for shuffling
        Random rng = new Random();

        //Necessary measurements to cast backgrounds to
        int screenHeight;
        int screenWidth;

        //Track if the game has been won/loss or is in the process
        bool IsGameWon;
        bool IsGameLoss;
        bool clickedAlready;
        bool isMovePossible;
       
        //Measurements of the cards to cut out from spritesheet
        int cardHeight;
        int cardWidth;

        //Make the cards slightly larger when being drawn
        double sizeModifier = 1.30;

        //Set up all card related variables (i.e. deck, board, etc.)
        List<int> deckOfCards = new List<int>();
        List<int>[,] board3D = new List<int> [2, 6];
        bool[,] isTopCardFace = new bool[2, 6];
        int[] cardsVisible = new int[11];
        int numFaceCards;

        //Track Mouse Variables
        MouseState curMouseState;
        MouseState prevMouseState;
        Vector2 MouseLoc;
        Vector2 mouseClickLoc;

        //Card Selection variables
        Vector2 selectedPile1;
        Vector2 selectedPile2;
        int? chosenCard1;
        int? chosenCard2;
        bool IsCard1Selected;
        bool IsCard2Selected;


        //Dest Rec's for card piles
        //Rectangle[] pileRectangles = new Rectangle[12];
        Rectangle[,] pileRectangles = new Rectangle [2,6];

        //Fonts
        SpriteFont regFont;
        SpriteFont titleFont;

        //Backgrounds and their rectangles
        Texture2D bgImg;
        Rectangle bgRec;
        Texture2D lossBgImg;
        Texture2D victoryBGImg;
        Rectangle lossVicRec;

        //Texture for the reset/play buttons
        Texture2D squareImg;

        //Images and Rectangles necessary for the cards
        Texture2D cardsImg;
        Texture2D cardBackImg;
        Rectangle pileCardRec;

        //Click to go to the next screen from title screen
        Rectangle nextScreenRec;

        string currentGameState = "titleScreen";
        Texture2D titleScreenImg;


        public Main()
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

            //Fonts used throughtout the game (although some of these have been breaking...)
            regFont = Content.Load<SpriteFont>("Fonts/TitleFont");
            titleFont = Content.Load<SpriteFont>("Fonts/titleScreen");

            //Images used for the backgrounds and the rectangles that they are drawn to.
            bgImg = Content.Load<Texture2D>("images/backgrounds/table");
            titleScreenImg = Content.Load<Texture2D>("images/backgrounds/TitleScreen");
            lossBgImg = Content.Load<Texture2D>("images/backgrounds/youlost");
            victoryBGImg = Content.Load<Texture2D>("images/backgrounds/youwon");
            bgRec = new Rectangle(0, 0, screenWidth, screenHeight);

            //Card images
            cardsImg = Content.Load<Texture2D>("images/sprites/CardFaces");
            cardBackImg = Content.Load<Texture2D>("images/sprites/CardBack");
            
            //Button image
            squareImg = Content.Load<Texture2D>("images/sprites/Square");

            //Setting up the card measurements from the larger spritesheet by cutting it up.
            cardWidth = cardsImg.Width / 13;
            cardHeight = cardsImg.Height / 4;

            //Rectangles that function as buttons that the user clicks to navigate between screens
            nextScreenRec = new Rectangle(100, screenHeight /2, 2*cardWidth, (int)(0.5*cardHeight));
            lossVicRec = new Rectangle(450, 180, ((int)4.5*cardWidth), (int)(0.5 * cardHeight));

            //The rectangle where the deck of cards image is drawn to.
            pileCardRec = new Rectangle(100, (int)(screenHeight-(cardHeight*1.5)), (int)(cardWidth*sizeModifier), (int)(cardHeight * sizeModifier));

            ResetGame();
            

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
            //Grab the Users mouse details to then use for click logic, always running
            UserMouseDetails();

            //On the title screen, just check whether the user has clicked on the next screenbutton
            if (currentGameState == "titleScreen")
            {
                if (curMouseState.LeftButton == ButtonState.Pressed && nextScreenRec.Contains(MouseLoc))
                {
                    currentGameState = "gameScreen";


                }
            }

            //Once on the game screen, track if the user has won or lost, as well as controlling the screen that the user goes to after the game ends.
            if (currentGameState == "gameScreen")
            {
                
                CheckWinCondition();
                CheckLossConditions();


                //If the user has achieved the victory state, go to victory screen.
                if (IsGameWon == true)
                {
                    currentGameState = "victory";
                }

                //If the user has failed to win, go to the loss screen.
                if (IsGameLoss == true)
                {
                    currentGameState = "loss";
                }
            }

            //If the user is on the victory screen, check if they pressed the title screen button.
            if (currentGameState == "victory")
            {
                //When the title screen button is pressed, reset all the variables
                if (curMouseState.LeftButton == ButtonState.Pressed && lossVicRec.Contains(MouseLoc))
                {
                    //Reset the game and reset the game state to title screen.
                    ResetGame();
                    currentGameState = "titleScreen";

                }
            }

            //The same idea as victory screen. Check if the user has clicked on the title screen button.
            if (currentGameState == "loss")
            {
                //Once they click reset everything.
                if (curMouseState.LeftButton == ButtonState.Pressed && lossVicRec.Contains(MouseLoc))
                {
                    ResetGame();
                    currentGameState = "titleScreen";

                }
            }


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

           
            //The drawing of the title screen background, text, and buttons
            if (currentGameState == "titleScreen")
            {
                //The background and title
                spriteBatch.Draw(titleScreenImg, bgRec, Color.Gray);
                spriteBatch.DrawString(regFont, "Elevens", new Vector2(100, 200), Color.White);

                //The button to play the game
                spriteBatch.Draw(squareImg, nextScreenRec, Color.BurlyWood);

                //The text on the button to be descriptive
                spriteBatch.DrawString(regFont, "play", new Vector2(nextScreenRec.X + (nextScreenRec.Width / 3) - 3, nextScreenRec.Y + 8), Color.Black);
                spriteBatch.DrawString(regFont, "play", new Vector2(nextScreenRec.X + (nextScreenRec.Width/3) - 5, nextScreenRec.Y + 6), Color.Azure);
            }

            //If the game state is on the actual game screen, draw the cards, new background 
            if (currentGameState == "gameScreen")
            {
                //Draw background first and then everything on top of it
                spriteBatch.Draw(bgImg, bgRec, Color.White);
                
                //If the game hasnt been decided yet (neither win nor loss yet), the game is considered running.
                if (!IsGameWon && !IsGameLoss)
                {
                    //Iterates through the board (Column by column) to draw the cards.
                    for (int boardColumn = 0; boardColumn < board3D.GetLength(1); ++boardColumn)
                    {
                        for (int boardRow = 0; boardRow < board3D.GetLength(0); ++boardRow)
                        {
                            //Calculations to draw the card
                            int cardSuit = ((board3D[boardRow, boardColumn][board3D[boardRow, boardColumn].Count - 1]) / 13);
                            int cardRank = ((board3D[boardRow, boardColumn][board3D[boardRow, boardColumn].Count - 1]) % 13);
                            int pileHeight = (board3D[boardRow, boardColumn].Count);

                            //Drawing the cards as normal
                            spriteBatch.Draw(cardsImg, pileRectangles[boardRow, boardColumn], new Rectangle((cardWidth * cardRank), cardHeight * cardSuit, cardWidth, cardHeight), Color.White);

                            //If the card rank is higher than a 10 and it isnt the only card in its pile, gray it out.
                            if (cardRank > 9 && pileHeight > 1)
                            {
                                spriteBatch.Draw(cardsImg, pileRectangles[boardRow, boardColumn], new Rectangle((cardWidth * cardRank), cardHeight * cardSuit, cardWidth, cardHeight), Color.Gray);
                            }

                            //if the mouse is hovering over the card, highlight it in yellow
                            if (pileRectangles[boardRow, boardColumn].Contains(MouseLoc))
                            {
                                spriteBatch.Draw(cardsImg, pileRectangles[boardRow, boardColumn], new Rectangle((cardWidth * cardRank), cardHeight * cardSuit, cardWidth, cardHeight), Color.LightYellow);
                            }

                        }

                    }
                }

                //If the deck is out of cards, it will not draw the deck of cards because there are no more cards left.
                if (deckOfCards.Count > 0)
                {
                    spriteBatch.Draw(cardBackImg, pileCardRec, Color.White);
                }
                //Wrote the number of cards left in the deck (that arent on the board)
                spriteBatch.DrawString(regFont, Convert.ToString(deckOfCards.Count), new Vector2(pileCardRec.X + (cardWidth / 2) - 5, pileCardRec.Y + (cardHeight / 2) - 10), Color.White);
            }

            //Draw the background and button if the user wins
            if (currentGameState == "victory")
            {
                spriteBatch.Draw(victoryBGImg, bgRec, Color.White);
                spriteBatch.Draw(squareImg, lossVicRec, Color.White);
                spriteBatch.DrawString(regFont, "back to title screen", new Vector2(lossVicRec.X + 10, lossVicRec.Y + 8), Color.Black);
            }

            //Draw the background and button if the user loses (different than the victory screen)
            if (currentGameState == "loss")
            { 
                spriteBatch.Draw(lossBgImg, bgRec, Color.White);
                spriteBatch.Draw(squareImg, lossVicRec, Color.White);
                spriteBatch.DrawString(regFont, "back to title screen", new Vector2(lossVicRec.X + 10, lossVicRec.Y + 8), Color.Black);
            }
            

            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        /// <summary>
        /// Subprogram that sets up the cards from 0-51 that is then used to calculate its suit and rank
        /// </summary>
        public void CardInit()
        {
            for (int value = 0; value < 52; ++value)
            {
                deckOfCards.Add(value);
            }
        }
        

        /// <summary>
        /// This subprogram initializes the lists that the board uses as piles.
        /// </summary>        
        public void LoadBoardLists()
        {
            //Iterate through the board and intialize each spot as a list and a rectangle (one is for logic, one is for the drawing aspect)
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    //For calculations.
                    board3D[boardrow,boardcolumn] = new List<int>();
                    
                    //For destination rectangles to which the cards will be drawn to.
                    pileRectangles[boardrow, boardcolumn] = new Rectangle();
                }
            }
        }

        /// <summary>
        /// Loads the possible card value that could be on top. These values are less than the face cards as those should return false to calculate victory condition.
        /// </summary>
        public void LoadPossibleTopValues()
        {
            for (int i = 0; i < 10; ++i)
            {
                cardsVisible[i] = 0;
            }
        }

        /// <summary>
        /// Iterates through the board and takes the bottom card of the deck and adds it to the board, before removing it from the deck of cards
        /// </summary>
        public void SetUpCardsFromDeck()
        {
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    //Add to each pile and remove from the deck after adding to the board.
                    board3D[boardrow, boardcolumn].Add(deckOfCards[0]);
                    deckOfCards.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// This subprogram defines the location and the size of the rectangles to which the cards will be drawn to.
        /// </summary>
        public void DefineDestRecs()
        {
            //Iterate through the rows then the columns, so the rectangles are initizalized by columns. 
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    //The location of the rectangle depends on its value in the board3D array.
                    pileRectangles[boardrow, boardcolumn] = new Rectangle((boardcolumn * cardWidth * 2) + 100, (boardrow * cardHeight * 2) + 100, Convert.ToInt32(cardWidth * sizeModifier), Convert.ToInt32(cardHeight * sizeModifier));
                }
            }
        }

        /// <summary>
        /// Subprogram that handles card shuffling logic
        /// </summary>
        public void ShuffleCards()
        {
            //Shuffle 10,000 times to be sure its shuffled
            for (int i = 0; i < Math.Pow(10, 4); ++i)
            {
                for (int j = 0; j < deckOfCards.Count; ++j)
                {
                    //Take a random index and set the second card to the random index, while the first is the next down.
                    int firstCard = deckOfCards[j];
                    int randomIndex = rng.Next(deckOfCards.Count);
                    int secondCard = deckOfCards[randomIndex];

                    deckOfCards[randomIndex] = firstCard;
                    deckOfCards[j] = secondCard;
                }
                
            }
        }

        /// <summary>
        /// Track the details of the users mouse (location, clicks) and pass that information to the card calculation if the user clicked)
        /// </summary>
        public void UserMouseDetails()
        {

            //Tracking basic logic of the mouse (location and making sure double clicking doesnt happen)
            prevMouseState = curMouseState;
            curMouseState = Mouse.GetState();
            MouseLoc.X = curMouseState.X;
            MouseLoc.Y = curMouseState.Y;

            //If the mouse is clicked and its on the game screen, pass information to calculation of cards
            if ((curMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed) && currentGameState == "gameScreen")
            {
                mouseClickLoc.X = MouseLoc.X;
                mouseClickLoc.Y = MouseLoc.Y;

                //Pass the location (a vector2) to the logic subprogram
                CardClickLogic(mouseClickLoc);
                

            }
            
        }
        
        /// <summary>
        /// This is the beast of the program. Handles all the calculations for whether the cards add up or not and whether cards can be clicked 
        /// on in the first place. Handles all the logic after a successful pair.
        /// </summary>
        /// <param name="mouseClickLoc"></param>
        public void CardClickLogic(Vector2 mouseClickLoc)
        {
            //Reset the click variable, so the program doesn't hop from selecting the second card
            //back to the first card.
            clickedAlready = false;

            //Handling the selection of the second card first, so a successful first selection wont double count.
            if (IsCard1Selected == true)
            {
                //The vector2 of the second selected pile sent to ClickQuadrant to see which pile is clicked on
                selectedPile2 = ClickQuadrant(mouseClickLoc);

                //Vector2(-1,-1) indicates an invalid pile selection, so if there is a valid selection, continue.
                if (selectedPile2 != new Vector2(-1, -1))
                {
                    //triggers the bool that says both cards are selected
                    IsCard2Selected = true;

                    //The second card chosen is the board[x,y][top card]
                    chosenCard2 = board3D[(int)selectedPile2.X, (int)selectedPile2.Y][board3D[(int)selectedPile2.X, (int)selectedPile2.Y].Count - 1];
                    int pileHeight = (board3D[(int)selectedPile2.X, (int)selectedPile2.Y].Count);

                    //If the card rank is greater than a 10, remove that card, provided it is the first card in the pile.
                    if (chosenCard2 % 13 > 9 && pileHeight == 1)
                    {
                        //Remove it and place back in pile
                        board3D[(int)selectedPile2.X, (int)selectedPile2.Y].Add(deckOfCards[0]);
                        deckOfCards.RemoveAt(0);

                        //Add a new card in its place
                        deckOfCards.Add(board3D[(int)selectedPile2.X, (int)selectedPile2.Y][0]);
                        board3D[(int)selectedPile2.X, (int)selectedPile2.Y].RemoveAt(0);
                    }

                    //If the first card and second add up to 11, add two more cards on top. 
                    if (IsValidCardCombination(chosenCard1, chosenCard2) == true)
                    {
                        //Add to pile 1
                        board3D[(int)selectedPile1.X, (int)selectedPile1.Y].Add(deckOfCards[0]);
                        deckOfCards.RemoveAt(0);

                        //Add to pile 2
                        board3D[(int)selectedPile2.X, (int)selectedPile2.Y].Add(deckOfCards[0]);
                        deckOfCards.RemoveAt(0);

                        //Reset the selected piles
                        selectedPile1 = new Vector2(-1, -1);
                        selectedPile2 = new Vector2(-1, -1);

                        //Remove card value
                        chosenCard1 = null;
                        chosenCard2 = null;                     

                        //No cards are selected anymore
                        IsCard1Selected = false;
                        IsCard2Selected = false;

                        prevMouseState = curMouseState;

                    }

                    //if they arent a valid pair, do nothing but reset the clicking variables
                    if (IsValidCardCombination(chosenCard1, chosenCard2) == false)
                    {
                        prevMouseState = curMouseState;
                        chosenCard1 = null;
                        chosenCard2 = null;

                        IsCard1Selected = false;
                        IsCard2Selected = false;
                        


                    }
                }

                //if not a valid pile selection, reset both cards (essentially making sure that the user isnt stuck with a bad first card)
                if (selectedPile2 == new Vector2(-1, -1))
                {
                   
                    chosenCard1 = -1;
                    chosenCard2 = -1;

                    IsCard1Selected = false;
                    IsCard2Selected = false;
                }

                clickedAlready = true;
            }

            //First card hasn't been chsosen yet and user hasnt clicked yet (prevents from overflow from selecting second card and then reseting)
            //Same logic as above, just for the first pile selected
            if (IsCard1Selected == false && clickedAlready == false)
            {
                selectedPile1 = ClickQuadrant(mouseClickLoc);

                //Validity of pile selected
                if (selectedPile1 != new Vector2(-1, -1))
                {
                    //Getting the card value from the pile selected.
                    chosenCard1 = board3D[(int)selectedPile1.X, (int)selectedPile1.Y][board3D[(int)selectedPile1.X, (int)selectedPile1.Y].Count - 1];
                    
                    //if the card is higher than 10 and first in pile, remove it 
                    if ((chosenCard1 % 13 > 9)
                        && board3D[(int)selectedPile1.X, (int)selectedPile1.Y].Count - 1 == 0)
                    {
                        board3D[(int)selectedPile1.X, (int)selectedPile1.Y].Add(deckOfCards[0]);
                        deckOfCards.RemoveAt(0);

                        deckOfCards.Add(board3D[(int)selectedPile1.X, (int)selectedPile1.Y][0]);
                        board3D[(int)selectedPile1.X, (int)selectedPile1.Y].RemoveAt(0);
                    }

                    IsCard1Selected = true;
                }
                clickedAlready = true;
            }

        }

        /// <summary>
        /// Function is called when user clicks to return which quadrant/pile they clicked in.
        /// </summary>
        /// <param name="MouseXY"></param>
        /// <returns></returns>
        public Vector2 ClickQuadrant(Vector2 MouseXY)
        {
            //Iterate through the board until the rectangles contain the mouse click point, otherwise return invalid click
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    if (pileRectangles[boardrow, boardcolumn].Contains(MouseXY))
                    {
                        return new Vector2(boardrow, boardcolumn);
                    }
                }
            }

            return new Vector2(-1, -1);
        }

        /// <summary>
        /// Function that returns whether the selected cards add up to 11. 
        /// Handles face cards as invalid, and returns false.
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <returns></returns>
        public bool IsValidCardCombination(int? card1, int? card2)
        {

            if (card1 % 13 > 9 || card2 % 13 > 9)
            {
                card1 = 0;
                card2 = 0;
            }
            int? sumOfSelectedCards = (card1 + card2) % 13;

            //9 is the same as a ten and an ace adding up (10 = index 9, ace = index 0)
            if (sumOfSelectedCards == 9)
            {
                return true;
            }

            else
            {
                return false;
            }
            
        }

        /// <summary>
        /// Subprogram that creates lists that indicates whether the top card is face card
        /// Once all of the cards are face cards, its a victory, but thats not handled here.
        /// </summary>
        public void LoadWinCheckList()
        {
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                    isTopCardFace[boardrow, boardcolumn] = false;
                }
            }
        }
        
        /// <summary>
        /// Subprogram iterates through the board and checks if the top card is a face card.
        /// Once all 12 piles have a facecard on top, the game is won.
        /// </summary>
        public void CheckWinCondition()
        {
            numFaceCards = 0;
            for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
            {
                for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                {
                   int topOfPileCard = board3D[boardrow, boardcolumn][board3D[boardrow, boardcolumn].Count - 1];

                   if ((topOfPileCard % 13) > 9)
                   {
                        isTopCardFace[boardrow, boardcolumn] = true;
                   }

                   if (isTopCardFace[boardrow, boardcolumn] == true)
                   {
                        numFaceCards++;
                        if (numFaceCards == 12)
                        {
                            Console.WriteLine("winner");
                            IsGameWon = true;
                        }
                        
                   }
                   
                }
            }

        }

        /// <summary>
        /// This subprogram runs throught the entire game checking if there are no more possible moves.
        /// Does addition and if no combinations equal 11, the game is lost.
        /// </summary>
        public void CheckLossConditions()
        {
            if (!IsGameWon)
            {
                //A variable that resets to the false at the beginning of each frame. If the for loops are run through
                //and there are no pairs that add up to 11, the variable remains false and the game ends.
                isMovePossible = false;

                //The outer two for loops just iterate through the game board
                //The inner two for loops iterate through the game board again and check if the current box adds to any other box to make 11.
                for (int boardcolumn = 0; boardcolumn < board3D.GetLength(1); ++boardcolumn)
                {
                    for (int boardrow = 0; boardrow < board3D.GetLength(0); ++boardrow)
                    {
                        //Get the value of the top card by taking the board index (X and Y) and taking that index's top card in the pile
                        int topOfPileCardValue = board3D[boardrow, boardcolumn][board3D[boardrow, boardcolumn].Count - 1] % 13;

                        //If its a face card, remove it from the calculation all together
                        if (topOfPileCardValue > 9)
                        {
                            topOfPileCardValue = -100;
                        }

                        //This runs through the board and compares the cards to the current "topOfPileCard" checking if they
                        //Add up to 11
                        for (int checkingBoardColumn = 0; checkingBoardColumn < board3D.GetLength(1); ++checkingBoardColumn)
                        {
                            for (int checkingBoardRow = 0; checkingBoardRow < board3D.GetLength(0); ++checkingBoardRow)
                            {
                                //Doesn't count the face cards.
                                int cardOnCheckingPile = board3D[checkingBoardRow, checkingBoardColumn][board3D[checkingBoardRow, checkingBoardColumn].Count - 1] % 13;
                                if (cardOnCheckingPile > 9)
                                {
                                    cardOnCheckingPile = -100;
                                }

                                //If the cards can add up, there are possible moves, therefore the game hasn't been lost.
                                if (((topOfPileCardValue + cardOnCheckingPile) % 13) == 9)
                                {
                                    isMovePossible = true;

                                }


                            }
                        }
                    }
                }

                //If the variable doesn't become true until here, it means there are no possible moves. The user has lost.
                if (isMovePossible == false)
                {
                    Console.WriteLine("loser");
                    IsGameLoss = true;

                }
            }
            
            
        }

        /// <summary>
        /// Reset all the cards in the deck
        /// </summary>
        public void RemoveDeckCards()
        {
            for (int i = 0; i < 52; ++i)
            {
                deckOfCards.Remove(i);
            }
        }

        /// <summary>
        /// This subprogram resest all the variables that are necessary for the game to run
        /// Everything is reset to default values.
        /// </summary>
        public void ResetGame()
        {
            IsGameLoss = false;
            IsGameWon = false;

            RemoveDeckCards();
            LoadBoardLists();
            LoadWinCheckList();
            LoadPossibleTopValues();
            CardInit();
            ShuffleCards();
            SetUpCardsFromDeck();
            DefineDestRecs();
        }
    }
}
