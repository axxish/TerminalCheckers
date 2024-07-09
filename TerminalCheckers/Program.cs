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
    char empty = '0';
    char plr1 = '1';
    char plr2 = '2';
    ConsoleColor defColor;
    ConsoleColor defBgColor;
    int bg1Color = 233;
    int bg1ColorSelected = 24;
    int bg2Color = 237;
    int bg2ColorSelected = 24;
    int p1Color = 0;
    int p2Color = 124;
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

    public void Draw()
    {

        for (int i = 0; i < board.GetLength(0); i++)
        {
            Console.Write("\u001b[38;5;4m" + (i) + " ");
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



                if (board[i, j] == '1')
                {
                    Console.Write("\u001b[38;5;" + p1Color + "m");
                    Console.Write("◉︎ ");
                }
                else if (board[i, j] == '2')
                {
                    Console.Write("\u001b[38;5;" + p2Color + "m");
                    Console.Write("◉︎ ");
                }
                else if (board[i, j] == 'x')
                {
                    Console.Write("\u001b[38;5;" + 222 + "m ?");
                }
                else
                {
                    bool needsSpace = true;
                    foreach (coords k in moveHighlights)
                    {
                        if (i == k.y && j == k.x)
                        {
                            Console.Write("\u001b[38;5;" + 222 + "m ?");
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
        Console.WriteLine("\u001b[38;5;4m  0 1 2 3 4 5 6 7");
        Console.ForegroundColor = defColor;
        Console.BackgroundColor = defBgColor;
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

                if (board[middleY, middleX] != '0' && board[middleY, middleX] != playerPiece)
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
                if (board[i, j] == playerPiece && CanJump(j, i, playerPiece))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool MovePiece(int startX, int startY, int endX, int endY, char playerPiece)
    {
        //regular figure
        if (endX >= boardSize || endY >= boardSize || board[endY, endX] != '0')
        {
            return false;
        }
        //regular move
        int deltaX = endX - startX;
        int deltaY = endY - startY;
        int desiredYDelta = -1;
        if (playerPiece == '1')
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
            return true;
        }


        //jump move
        if (Math.Abs(deltaX) == 2 && Math.Abs(deltaY) == 2)
        {
            int middleX = startX + deltaX / 2;
            int middleY = startY + deltaY / 2;

            if (board[middleY, middleX] != '0' && board[middleY, middleX] != playerPiece)
            {
                board[endY, endX] = playerPiece;
                board[startY, startX] = '0';
                board[middleY, middleX] = '0';
                lastMoveWasJump = true;
                return true;
            }
        }


        return false;
    }

    void showMoves(int startX, int startY, char playerPiece)
    {
        int desiredYDelta = -1;
        if (playerPiece == '1')
        {
            desiredYDelta = 1;
        }
        moveHighlights.Add(new coords(startX + 1, startY + desiredYDelta));
        moveHighlights.Add(new coords(startX - 1, startY + desiredYDelta));
    }

    public void Start()
    {
        Init();
        currPlayer = '2';
        moveState state = moveState.pieceSelection;
        cursorY = 5;
        cursorX = 0;
        while (true)
        {
            Console.Clear();

            Draw();
            Console.Write(message);
            message = "";
            //Console.WriteLine(currPlayer);
            Console.WriteLine((currPlayer == '2') ? "Red's turn" : "Black's turn");
            if (state == moveState.pieceSelection)
            {
                Console.Write("Select the piece ( WASD to move cursor, X to select)");
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

                        if (board[cursorY, cursorX] != currPlayer)
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
                                if (currPlayer == '2') { currPlayer = '1'; }
                                else
                                if (currPlayer == '1') { currPlayer = '2'; };

                            }
                            state = moveState.pieceSelection;
                            moveHighlights = new ArrayList();
                        }
                        else
                        {
                            message += "\u001b[38;5;124mIncorrect move!\n";
                        }
                        break;
                    case 'c':
                        if (currPlayer == '2')
                        {
                            if (((chosenPieceY - 1) > 0) && ((chosenPieceX + 1) < board.GetLength(0)))
                            {
                                if (board[chosenPieceY - 1, chosenPieceX + 1] == 'x')
                                    board[chosenPieceY - 1, chosenPieceX + 1] = '0';
                            }

                            if (((chosenPieceY - 1) > 0) && ((chosenPieceX - 1) > 0))
                            {
                                if (board[chosenPieceY - 1, chosenPieceX - 1] == 'x')
                                    board[chosenPieceY - 1, chosenPieceX - 1] = '0';
                            }
                        }

                        state = moveState.pieceSelection;
                        moveHighlights = new ArrayList();
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