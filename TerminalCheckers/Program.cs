// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Authentication;


class Game
{
    char[,] board;
    int boardSize = 8;

    enum moveState
    {
        pieceSelection,
        moveSelection
    }

    int cursorX = 0;
    int cursorY = 0;

    int chosenPieceX = 0;
    int chosenPieceY = 0;
    char currPlayer;
    char currPlayerQueen;
    char empty = '0';
    char plr1 = 'b';
    char plr2 = 'r';
    ConsoleColor defColor;
    ConsoleColor defBgColor;
    int bg1Color = 233;
    int bg1ColorSelected = 24;
    int bg2Color = 237;
    int bg2ColorSelected = 24;
    int p1Color = 254;
    int p2Color = 9;
    int highlightColor = 25;
    bool lastMoveWasJump = false;

    struct coords
    {
        public coords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x;
        public int y;
    }
    ArrayList moveHighlights = new ArrayList();

    String message;

    void Init()
    {
        defColor = Console.ForegroundColor;
        defBgColor = Console.BackgroundColor;

        board = new char[boardSize, boardSize];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (i < 3 && (i + j) % 2 != 0)
                    board[i, j] = plr1; // Black pieces
                else if (i > 4 && (i + j) % 2 != 0)
                    board[i, j] = plr2; // Red pieces
                else
                    board[i, j] = empty; // Empty squares
            }
        }

    }

    void EmptyBoard()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                board[i, j] = empty; // Empty squares
            }
        }
    }

    public void Draw()
    {

        for (int i = 0; i < board.GetLength(0); i++)
        {
            Console.Write("\u001b[38;5;"+highlightColor+"m" + (i) + " ");
            for (int j = 0; j < board.GetLength(0); j++)
            {

                if ((i + j) % 2 != 0)
                {
                    if (i == cursorY && j == cursorX)
                    {
                        Console.Write("\u001b[48;5;" + bg1ColorSelected + "m");
                    }
                    else
                    {
                        Console.Write("\u001b[48;5;" + bg1Color + "m");
                    }
                }
                else
                {
                    if (i == cursorY && j == cursorX)
                    {
                        Console.Write("\u001b[48;5;" + bg2ColorSelected + "m");
                    }
                    else
                    {
                        Console.Write("\u001b[48;5;" + bg2Color + "m");
                    }
                }



                if (board[i, j] == 'b')
                {
                    Console.Write("\u001b[38;5;" + p1Color + "m");
                    Console.Write("◉︎");
                }
                else if (board[i, j] == 'r')
                {
                    Console.Write("\u001b[38;5;" + p2Color + "m");
                    Console.Write("◉︎");
                }
                else if (board[i, j] == 'B')
                {
                    Console.Write("\u001b[38;5;" + p1Color + "m");
                    Console.Write("♛ ");
                }
                else if (board[i, j] == 'R')
                {
                    Console.Write("\u001b[38;5;" + p2Color + "m");
                    Console.Write("♛ ");
                }

                else
                {
                    bool needsSpace = true;
                    bool alreadyDrawn = false;
                    foreach (coords k in moveHighlights)
                    {
                        if (i == k.y && j == k.x)
                        {
                            if (!alreadyDrawn)
                            {
                                Console.Write("\u001b[38;5;" + 222 + "m ?");
                                alreadyDrawn = true;
                            }
                            needsSpace = false;
                        }
                    }
                    if (needsSpace)
                        Console.Write("  ");
                }



            }
            Console.BackgroundColor = defBgColor;
            Console.WriteLine();
        }
        Console.WriteLine("\u001b[38;5;"+highlightColor+"m  0 1 2 3 4 5 6 7");
        Console.ForegroundColor = defColor;
        Console.BackgroundColor = defBgColor;
    }

    private void PromoteToQueen(int x, int y, char playerPiece)
    {
        if (playerPiece == 'r' && y == 0)
        {
            board[y, x] = 'R'; // Red queen
        }
        else if (playerPiece == 'b' && y == boardSize - 1)
        {
            board[y, x] = 'B'; // Black queen
        }
    }


    private bool CanJump(int x, int y, char playerPiece)
    {
        int[] dx = { -2, -2, 2, 2 };
        int[] dy = { -2, 2, -2, 2 };

        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];

            if (newX >= 0 && newX < board.GetLength(0) && newY >= 0 && newY < board.GetLength(1) &&
                board[newY, newX] == '0')
            {
                int middleX = x + dx[i] / 2;
                int middleY = y + dy[i] / 2;

                if (board[middleY, middleX] != '0' && board[middleY, middleX] != playerPiece && board[middleY, middleX] != currPlayerQueen)
                {
                    message += "Can jump at " + x + " " + y + " to " + newX + " " + newY + "\n";
                    return true;
                }
            }
        }
        return false;
    }

    private bool HasForcedJump(char playerPiece)
    {
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (((board[i, j] == playerPiece) || (board[i, j] == currPlayerQueen)) && CanJump(j, i, playerPiece))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool MovePiece(int startX, int startY, int endX, int endY, char playerPiece)
    {

        bool isQueen = (board[startY, startX] == currPlayerQueen);
        message += isQueen + "\n";


    
        if (endX >= boardSize || endY >= boardSize || board[endY, endX] != '0')
        {
            return false;
        }
        
        int deltaX = endX - startX;
        int deltaY = endY - startY;
        
        if (deltaX == 0 || deltaY == 0)
        {
            return false;
        }

		//jump move
		if (Math.Abs(deltaX) == 2 && Math.Abs(deltaY) == 2)
		{
			message += "The attempted move was a jump move \n";
			int middleX = startX + deltaX / 2;
			int middleY = startY + deltaY / 2;

			if (board[middleY, middleX] != '0' && board[middleY, middleX] != playerPiece && board[middleY, middleX] != currPlayerQueen)
			{
                if (isQueen)
                {
                    board[endY, endX] = currPlayerQueen;
                }
                else
                {
                    board[endY, endX] = playerPiece;
                }
				board[startY, startX] = '0';
				board[middleY, middleX] = '0';
				lastMoveWasJump = true;
				PromoteToQueen(endX, endY, playerPiece);
				return true;
			}
		}

		//regular move
		if (!isQueen)
        {
            int desiredYDelta = -1;
            if (playerPiece == 'b')
            {
                desiredYDelta = 1;
            }

            if (Math.Abs(deltaX) == 1 && deltaY == desiredYDelta && board[endY, endX] == '0')
            {
                if (HasForcedJump(playerPiece))
                {
                    return false;
                }
                board[endY, endX] = playerPiece;
                board[startY, startX] = '0';
                lastMoveWasJump = false;
                PromoteToQueen(endX, endY, playerPiece);
                return true;
            }
        }
        //queen move
        else
        {
            if (HasForcedJump(playerPiece)){
                return false;
            }
            if (Math.Abs(deltaX) != Math.Abs(deltaY)) { return false; }
            
            int stepX = deltaX / Math.Abs(deltaX);
            int stepY = deltaY / Math.Abs(deltaY);

            for (int i = 1; i < Math.Abs(deltaX); i++)
            {
                if (board[startY + i * stepY, startX + i * stepX] != '0')
                {
                    return false;
                }
            }

            if (currPlayer == 'r')
            {
                board[endY, endX] = 'R';
            }
            else
            {
                board[endY, endX] = 'B';
            }
			lastMoveWasJump = false;
			board[startY, startX] = '0';
            return true;
        }
        

        


        return false;
    }

    void showMoves_(int startX, int startY, char playerPiece)
    {
        int desiredYDelta = -1;
        if (playerPiece == 'b')
        {
            desiredYDelta = 1;
        }
        moveHighlights.Add(new coords(startX + 1, startY + desiredYDelta));
        moveHighlights.Add(new coords(startX - 1, startY + desiredYDelta));
        
    }

	public void showMoves(int startX, int startY, char playerPiece)
	{
		moveHighlights.Clear();
		char piece = board[startY, startX];

		if (piece == '0' || (piece != 'r' && piece != 'b' && piece != 'R' && piece != 'B'))
		{
			return ;
		}

		bool isQueen = piece == 'R' || piece == 'B';
		char opponentPiece = piece == 'r' || piece == 'R' ? 'b' : 'r';
		char opponentQueen = piece == 'r' || piece == 'R' ? 'B' : 'R';

		// Define directions for normal and jump moves
		int[] normalDx, normalDy, jumpDx, jumpDy;

		if (isQueen)
		{
			normalDx = new int[] { -1, -1, 1, 1 };
			normalDy = new int[] { -1, 1, -1, 1 };
		}
		else if (piece == 'r')
		{
			normalDx = new int[] { 1, -1 };
			normalDy = new int[] { -1, -1 };
		}
		else // piece == 'b'
		{
			normalDx = new int[] { 1, -1 };
			normalDy = new int[] { 1, 1 };
		}

		// Jumps can be in all directions for non-queens
		jumpDx = new int[] { -2, -2, 2, 2 };
		jumpDy = new int[] { -2, 2, -2, 2 };

		// Regular moves for non-queens
		for (int i = 0; i < normalDx.Length; i++)
		{
			int newX = startX + normalDx[i];
			int newY = startY + normalDy[i];
			if (newX >= 0 && newX < boardSize && newY >= 0 && newY < boardSize && board[newY, newX] == '0')
			{
                if (!HasForcedJump(playerPiece))
                {
                    moveHighlights.Add(new coords(newX, newY));
                }
			}
		}

		// Jump moves for all pieces
		for (int i = 0; i < jumpDx.Length; i++)
		{
			int jumpX = startX + jumpDx[i];
			int jumpY = startY + jumpDy[i];
			int middleX = startX + jumpDx[i] / 2;
			int middleY = startY + jumpDy[i] / 2;
			if (jumpX >= 0 && jumpX < boardSize && jumpY >= 0 && jumpY < boardSize &&
				(board[middleY, middleX] == opponentPiece || board[middleY, middleX] == opponentQueen) &&
				board[jumpY, jumpX] == '0')
			{
				moveHighlights.Clear();
				moveHighlights.Add(new coords(jumpX, jumpY));
			}
		}

		// Queen moves
		if (isQueen)
		{
			for (int i = 0; i < 4; i++)
			{
				int stepX = normalDx[i];
				int stepY = normalDy[i];
				int newX = startX + stepX;
				int newY = startY + stepY;

				while (newX >= 0 && newX < boardSize && newY >= 0 && newY < boardSize)
				{
					if (board[newY, newX] == '0')

					{
						if (!HasForcedJump(playerPiece))
							moveHighlights.Add(new coords(newX, newY));
					}
					else if (board[newY, newX] == opponentPiece || board[newY, newX] == opponentQueen)
					{
						int jumpX = newX + stepX;
						int jumpY = newY + stepY;
						if (jumpX >= 0 && jumpX < boardSize && jumpY >= 0 && jumpY < boardSize && board[jumpY, jumpX] == ' ')
						{
                            moveHighlights.Clear();
							moveHighlights.Add(new coords(jumpX, jumpY));
						}
						break;
					}
					else
					{
						break;
					}

					newX += stepX;
					newY += stepY;
				}
			}
		}

		return;
	}





public void Start()
    {
        Init();
        currPlayer = 'r';
        moveState state = moveState.pieceSelection;
        cursorY = 5;
        cursorX = 0;
        while (true)
        {
            Console.Clear();
            if (currPlayer == 'r')
            {
                currPlayerQueen = 'R';
            }
            else
            {
                currPlayerQueen = 'B';
            }
            Draw();
            Console.Write(message);
            message = "";
            //Console.WriteLine(currPlayer);
            Console.WriteLine((currPlayer == 'r') ? "Red's turn" : "Black's turn");
            if (state == moveState.pieceSelection)
            {
                Console.Write("Select the piece ( WASD to move cursor, X to select, N to start a new game)");
                ConsoleKeyInfo key = Console.ReadKey();
                char keyChar = key.KeyChar;

                switch (keyChar)
                {
                    case 'w':
                        if (cursorY > 0) cursorY--;
                        break;
                    case 's':
                        if (cursorY < 8) cursorY++;
                        break;
                    case 'a':
                        if (cursorX > 0) cursorX--;
                        break;
                    case 'd':
                        if (cursorX < 8) cursorX++;
                        break;
                    case 'b':
                        board[cursorY, cursorX] = currPlayerQueen;
                        break;
                    case 'v':
                        board[cursorY, cursorX] = '0';
                        break;
                    case 'n':
                        Init();
                        break;
                    case 'x':

                        if (board[cursorY, cursorX] != currPlayer && board[cursorY, cursorX] != currPlayerQueen)
                        {
                            message += "\u001b[38;5;124mIncorrect cell!\n";
                            Console.ForegroundColor = defColor;
                        }
                        else
                        {
                            chosenPieceX = cursorX;
                            chosenPieceY = cursorY;
                            showMoves(chosenPieceX, chosenPieceY, currPlayer);
                            state = moveState.moveSelection;
                        }
                        break;

                }

            }
            else if (state == moveState.moveSelection)
            {
                Console.Write("Select the cell to move. X to Confirm, C to cancel");
                ConsoleKeyInfo key = Console.ReadKey();
                char keyChar = key.KeyChar;

                switch (keyChar)
                {
                    case 'w':
                        if (cursorY > 0) cursorY--;
                        break;
                    case 's':
                        if (cursorY < 8) cursorY++;
                        break;
                    case 'a':
                        if (cursorX > 0) cursorX--;
                        break;
                    case 'd':
                        if (cursorX < 8) cursorX++;
                        break;
                    case 'x':
                        if (MovePiece(chosenPieceX, chosenPieceY, cursorX, cursorY, currPlayer))
                        {
                            if (HasForcedJump(currPlayer) && lastMoveWasJump)
                            {

                            }
                            else
                            {
                                if (currPlayer == 'r') { currPlayer = 'b'; }
                                else
                                if (currPlayer == 'b') { currPlayer = 'r'; };

                            }
                            state = moveState.pieceSelection;
                            moveHighlights.Clear();
                            
                        }
                        else
                        {
                            message += "\u001b[38;5;124mIncorrect move!\n";
                        }
                        break;
                    case 'c':


                        state = moveState.pieceSelection;
                        moveHighlights.Clear();
                        break;
                }
            }


        }
    }
}

class Prog
{

    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Game newGame = new Game();
        newGame.Start();
    }
}