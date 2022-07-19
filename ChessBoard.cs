using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chess
{
    internal class ChessBoard
    {
        private const byte EMPTY = 0;
        private const byte PAWN = 1;
        private const byte ROOK = 2;
        private const byte KNIGHT = 4;
        private const byte BISHOP = 8;
        private const byte QUEEN = 16;
        private const byte KING = 32;
        private const byte BLACK = 64;
        private const byte WHITE = 128;
        private (int, int)[] KNIGHTMOVES = { (2, -1), (2, 1), (1, -2), (1, 2), (-2, -1), (-2, 1), (-1, -2), (-1, 2) };
        private (int, int)[] KINGMOVES = { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };

        public string FEN;
        public byte[,] Board = new byte[0, 0];
        public bool isWhiteTurn;
        public List<(int, int, byte)> LegalMoves = new List<(int, int, byte)>();
        public bool isfinished;
        public byte Winner;
        public (int, int) BlackKingPos;
        public (int, int) WhiteKingPos;
        public List<(int, int)> PossibleCasts = new List<(int, int)>();
        public (int, int) EnPassant;
        public int HalfMoves;
        public int FullMoves;
        public List<string> MovesList = new List<string>();

        public ChessBoard()
        {
            FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0";
            fromFEN(FEN);
        }

        public ChessBoard(string Fen)
        {
            FEN = Fen;
            fromFEN(FEN);
        }

        public void fromFEN(string Fen)
        {
            Board = new byte[8, 8];
            PossibleCasts = new List<(int, int)>();
            isfinished = false;
            Winner = 1;
            MovesList = new List<string>();

            int y = 0;
            int x = 0;
            int val;
            foreach (char c in Fen.Split(" ")[0])
            {
                if (c == 'P') Board[y, x] = WHITE + PAWN;
                else if (c == 'p') Board[y, x] = BLACK + PAWN;
                else if (c == 'R') Board[y, x] = WHITE + ROOK;
                else if (c == 'r') Board[y, x] = BLACK + ROOK;
                else if (c == 'N') Board[y, x] = WHITE + KNIGHT;
                else if (c == 'n') Board[y, x] = BLACK + KNIGHT;
                else if (c == 'B') Board[y, x] = WHITE + BISHOP;
                else if (c == 'b') Board[y, x] = BLACK + BISHOP;
                else if (c == 'Q') Board[y, x] = WHITE + QUEEN;
                else if (c == 'q') Board[y, x] = BLACK + QUEEN;
                else if (c == 'K')
                {
                    Board[y, x] = WHITE + KING;
                    WhiteKingPos = (y, x);
                }
                else if (c == 'k')
                {
                    Board[y, x] = BLACK + KING;
                    BlackKingPos = (y, x);
                }

                if (c == '/') (y, x) = (y + 1, 0);
                else if (int.TryParse(c.ToString(), out val)) x += val;
                else x++;
            }

            isWhiteTurn = (Fen.Split(" ")[1] == "w");

            foreach (char c in Fen.Split(" ")[2])
            {
                if (c == 'K') PossibleCasts.Add((7, 2));
                else if (c == 'Q') PossibleCasts.Add((7, 6));
                else if (c == 'k') PossibleCasts.Add((0, 2));
                else if (c == 'q') PossibleCasts.Add((0, 6));
            }

            string StrEnPassant = Fen.Split(" ")[3];
            if (StrEnPassant != "-") EnPassant = (8 - Int32.Parse(StrEnPassant[1].ToString()), ((byte)StrEnPassant[0]) - 97);
            else EnPassant = (-1, -1);

            if (Fen.Split(" ").Length == 6)
            {
                HalfMoves = Int32.Parse(Fen.Split(" ")[4]);
                FullMoves = Int32.Parse(Fen.Split(" ")[5]);
            }
            else
            {
                HalfMoves = 0;
                FullMoves = 0;
            }
        }

        public string toFEN()
        {
            string fen = "";
            int compte_truc_vide = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (isEmpty(y, x)) compte_truc_vide++;
                    else
                    {
                        if (compte_truc_vide != 0)
                        {
                            fen += compte_truc_vide.ToString();
                            compte_truc_vide = 0;
                        }
                        byte ASCII = 65;
                        if (isBlack(y, x)) ASCII += 32;
                        if (isPawn(y, x)) ASCII += 15;
                        else if (isRook(y, x)) ASCII += 17;
                        else if (isKnight(y, x)) ASCII += 13;
                        else if (isBishop(y, x)) ASCII += 1;
                        else if (isQueen(y, x)) ASCII += 16;
                        else if (isKing(y, x)) ASCII += 10;
                        fen += (char)ASCII;
                    }
                }
                if (compte_truc_vide != 0)
                {
                    fen += compte_truc_vide.ToString();
                    compte_truc_vide = 0;
                }
                fen += "/";
            }
            fen = fen.Remove(fen.Length - 1, 1);

            if (isWhiteTurn) fen += " w ";
            else fen += " b ";

            foreach ((int, int) vel in PossibleCasts)
            {
                if (vel == (0, 2)) fen += "k";
                else if (vel == (0, 6)) fen += "q";
                else if (vel == (7, 2)) fen += "K";
                else if (vel == (7, 6)) fen += "Q";
            }
            if (fen[fen.Length - 1] == ' ') fen += '-';

            if (EnPassant == (-1, -1)) fen += " -";
            else fen += String.Format(" {0}{1}", (char)((byte)(97 + EnPassant.Item2)), 8 - EnPassant.Item1);

            fen += String.Format(" {0} {1}", HalfMoves, FullMoves);
            return fen;
        }

        public bool isEmpty(int y, int x) { return (Board[y, x] == EMPTY); }

        public bool isPawn(int y, int x) { return ((Board[y, x] & PAWN) == PAWN); }

        public bool isRook(int y, int x) { return ((Board[y, x] & ROOK) == ROOK); }

        public bool isKnight(int y, int x) { return ((Board[y, x] & KNIGHT) == KNIGHT); }

        public bool isBishop(int y, int x) { return ((Board[y, x] & BISHOP) == BISHOP); }

        public bool isQueen(int y, int x) { return ((Board[y, x] & QUEEN) == QUEEN); }

        public bool isKing(int y, int x) { return ((Board[y, x] & KING) == KING); }

        public bool isBlack(int y, int x) { return ((Board[y, x] & BLACK) == BLACK); }

        public bool isWhite(int y, int x) { return ((Board[y, x] & WHITE) == WHITE); }

        public bool sameColour(int y1, int x1, int y2, int x2) { return ((isBlack(y1, x1) && isBlack(y2, x2)) || (isWhite(y1, x1) && isWhite(y2, x2))); }


        public List<(int, int, byte)> pseudoMoves(int posY, int posX)
        {
            List<(int, int, byte)> PseudoMoves = new List<(int, int, byte)>();
            if (isEmpty(posY, posX)) return PseudoMoves;

            if (isPawn(posY, posX))
            {
                if (isWhite(posY, posX))
                {
                    if (posY > 1 && isEmpty(posY - 1, posX))
                    {
                        PseudoMoves.Add((-1, 0, EMPTY));
                        if (posY == 6 && isEmpty(posY - 2, posX)) PseudoMoves.Add((-2, 0, EMPTY));
                    }
                    else if (posY == 1)
                    {
                        if (isEmpty(0, posX))
                        {
                            PseudoMoves.Add((-1, 0, QUEEN));
                            PseudoMoves.Add((-1, 0, KNIGHT));
                            PseudoMoves.Add((-1, 0, ROOK));
                            PseudoMoves.Add((-1, 0, BISHOP));
                        }
                        if (posX > 0 && !isEmpty(0, posX - 1))
                        {
                            PseudoMoves.Add((-1, -1, QUEEN));
                            PseudoMoves.Add((-1, -1, KNIGHT));
                            PseudoMoves.Add((-1, -1, ROOK));
                            PseudoMoves.Add((-1, -1, BISHOP));
                        }
                        if (posX < 7 && !isEmpty(0, posX + 1))
                        {
                            PseudoMoves.Add((-1, 1, QUEEN));
                            PseudoMoves.Add((-1, 1, KNIGHT));
                            PseudoMoves.Add((-1, 1, ROOK));
                            PseudoMoves.Add((-1, 1, BISHOP));
                        }
                    }

                    if ((posY > 1 && posX > 0 && (!isEmpty(posY - 1, posX - 1) && !sameColour(posY, posX, posY - 1, posX - 1))) || EnPassant == (posY - 1, posX - 1)) PseudoMoves.Add((-1, -1, EMPTY));
                    if ((posY > 1 && posX < 7 && (!isEmpty(posY - 1, posX + 1) && !sameColour(posY, posX, posY - 1, posX + 1))) || EnPassant == (posY - 1, posX + 1)) PseudoMoves.Add((-1, 1, EMPTY));
                }
                else if (isBlack(posY, posX))
                {
                    if (posY < 6 && isEmpty(posY + 1, posX))
                    {
                        PseudoMoves.Add((1, 0, EMPTY));
                        if (posY == 1 && isEmpty(posY + 2, posX)) PseudoMoves.Add((2, 0, EMPTY));
                    }
                    else if (posY == 6)
                    {
                        if (isEmpty(7, posX))
                        {
                            PseudoMoves.Add((1, 0, QUEEN));
                            PseudoMoves.Add((1, 0, KNIGHT));
                            PseudoMoves.Add((1, 0, ROOK));
                            PseudoMoves.Add((1, 0, BISHOP));
                        }
                        if (posX > 0 && !isEmpty(7, posX - 1))
                        {
                            PseudoMoves.Add((1, -1, QUEEN));
                            PseudoMoves.Add((1, -1, KNIGHT));
                            PseudoMoves.Add((1, -1, ROOK));
                            PseudoMoves.Add((1, -1, BISHOP));
                        }
                        if (posX < 7 && !isEmpty(7, posX + 1))
                        {
                            PseudoMoves.Add((1, 1, QUEEN));
                            PseudoMoves.Add((1, 1, KNIGHT));
                            PseudoMoves.Add((1, 1, ROOK));
                            PseudoMoves.Add((1, 1, BISHOP));
                        }
                    }

                    if ((posY < 6 && posX > 0 && (!isEmpty(posY + 1, posX - 1) && !sameColour(posY, posX, posY + 1, posX - 1))) || EnPassant == (posY + 1, posX - 1)) PseudoMoves.Add((1, -1, EMPTY));
                    if ((posY < 6 && posX < 7 && (!isEmpty(posY + 1, posX + 1) && !sameColour(posY, posX, posY + 1, posX + 1))) || EnPassant == (posY + 1, posX + 1)) PseudoMoves.Add((1, 1, EMPTY));
                }
            }
            if (isRook(posY, posX) || isQueen(posY, posX))
            {
                for (int velX = 1; velX < 8; velX++)
                {
                    if (0 <= posX + velX && posX + velX < 8)
                    {
                        if (isEmpty(posY, posX + velX)) PseudoMoves.Add((0, velX, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY, posX + velX)) PseudoMoves.Add((0, velX, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
                for (int velX = -1; velX > -8; velX--)
                {
                    if (0 <= posX + velX && posX + velX < 8)
                    {
                        if (isEmpty(posY, posX + velX)) PseudoMoves.Add((0, velX, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY, posX + velX)) PseudoMoves.Add((0, velX, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
                for (int velY = 1; velY < 8; velY++)
                {
                    if (0 <= posY + velY && posY + velY < 8)
                    {
                        if (isEmpty(posY + velY, posX)) PseudoMoves.Add((velY, 0, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY + velY, posX)) PseudoMoves.Add((velY, 0, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
                for (int velY = -1; velY > -8; velY--)
                {
                    if (0 <= posY + velY && posY + velY < 8)
                    {
                        if (isEmpty(posY + velY, posX)) PseudoMoves.Add((velY, 0, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY + velY, posX)) PseudoMoves.Add((velY, 0, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
            }
            if (isKnight(posY, posX))
            {
                foreach ((int velY, int velX) in KNIGHTMOVES)
                {
                    if (0 <= posY + velY && posY + velY < 8 && 0 <= posX + velX && posX + velX < 8 && !sameColour(posY, posX, posY + velY, posX + velX)) PseudoMoves.Add((velY, velX, EMPTY));
                }
            }
            if (isBishop(posY, posX) || isQueen(posY, posX))
            {
                for (int vel = 1; vel < 8; vel++)
                {
                    if (posY + vel < 8 && posX + vel < 8)
                    {
                        if (isEmpty(posY + vel, posX + vel)) PseudoMoves.Add((vel, vel, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY + vel, posX + vel)) PseudoMoves.Add((vel, vel, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
                for (int vel = 1; vel < 8; vel++)
                {
                    if (vel <= posY && posX + vel < 8)
                    {
                        if (isEmpty(posY - vel, posX + vel)) PseudoMoves.Add((-vel, vel, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY - vel, posX + vel)) PseudoMoves.Add((-vel, vel, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
                for (int vel = 1; vel < 8; vel++)
                {
                    if (posY + vel < 8 && vel <= posX)
                    {
                        if (isEmpty(posY + vel, posX - vel)) PseudoMoves.Add((vel, -vel, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY + vel, posX - vel)) PseudoMoves.Add((vel, -vel, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
                for (int vel = 1; vel < 8; vel++)
                {
                    if (vel <= posY && vel <= posX)
                    {
                        if (isEmpty(posY - vel, posX - vel)) PseudoMoves.Add((-vel, -vel, EMPTY));
                        else
                        {
                            if (!sameColour(posY, posX, posY - vel, posX - vel)) PseudoMoves.Add((-vel, -vel, EMPTY));
                            break;
                        }
                    }
                    else break;
                }
            }
            if (isKing(posY, posX))
            {
                foreach ((int velY, int velX) in KINGMOVES)
                {
                    if (0 <= posY + velY && posY + velY < 8 && 0 <= posX + velX && posX + velX < 8 && !sameColour(posY, posX, posY + velY, posX + velX)) PseudoMoves.Add((velY, velX, EMPTY));
                }

                if (isWhite(posY, posX) && PossibleCasts.Contains((7, 6)) && !ismenaced((posY, posX), BLACK) && isEmpty(posY, posX + 1) && isEmpty(posY, posX + 2) && !ismenaced((posY, posX + 1), BLACK) && !ismenaced((posY, posX + 2), BLACK)) PseudoMoves.Add((0, 2, EMPTY));
                else if (isBlack(posY, posX) && PossibleCasts.Contains((0, 6)) && !ismenaced((posY, posX), WHITE) && isEmpty(posY, posX + 1) && isEmpty(posY, posX + 2) && !ismenaced((posY, posX + 1), WHITE) && !ismenaced((posY, posX + 2), WHITE)) PseudoMoves.Add((0, 2, EMPTY));
                if (isWhite(posY, posX) && PossibleCasts.Contains((7, 2)) && !ismenaced((posY, posX), BLACK) && isEmpty(posY, posX - 1) && isEmpty(posY, posX - 2) && isEmpty(posY, posX - 3) && !ismenaced((posY, posX - 1), BLACK) && !ismenaced((posY, posX - 2), BLACK)) PseudoMoves.Add((0, -2, EMPTY));
                else if (isBlack(posY, posX) && PossibleCasts.Contains((0, 2)) && !ismenaced((posY, posX), WHITE) && isEmpty(posY, posX - 1) && isEmpty(posY, posX - 2) && isEmpty(posY, posX - 3) && !ismenaced((posY, posX - 1), WHITE) && !ismenaced((posY, posX - 2), WHITE)) PseudoMoves.Add((0, -2, EMPTY));
            }

            return PseudoMoves;
        }

        public bool ismenaced((int, int) pos, byte color)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if ((Board[y, x] & color) == color && !isKing(y, x))
                    {
                        List<(int, int, byte)> PseudoMoves = pseudoMoves(y, x);
                        if (PseudoMoves.Contains((pos.Item1 - y, pos.Item2 - x, EMPTY))) return true;
                        if (isPawn(y, x))
                        {
                            if (PseudoMoves.Contains((pos.Item1 - y, pos.Item2 - x, QUEEN))) return true;
                            if (PseudoMoves.Contains((pos.Item1 - y, pos.Item2 - x, KNIGHT))) return true;
                            if (PseudoMoves.Contains((pos.Item1 - y, pos.Item2 - x, ROOK))) return true;
                            if (PseudoMoves.Contains((pos.Item1 - y, pos.Item2 - x, BISHOP))) return true;
                        }
                    }
                }
            }
            return false;
        }

        public List<(int, int, byte)> legalMoves(int posY, int posX)
        {
            LegalMoves = pseudoMoves(posY, posX);
            foreach ((int velY, int velX, byte promotion) in LegalMoves.ToList())
            {
                int newY = posY + velY;
                int newX = posX + velX;
                if (isKing(newY, newX)) LegalMoves.Remove((velY, velX, promotion));
                else
                {
                    byte temp1 = Board[posY, posX];
                    byte temp2 = Board[newY, newX];
                    Board[newY, newX] = Board[posY, posX];
                    Board[posY, posX] = EMPTY;
                    if (isPawn(newY, newX))
                    {
                        if (newY == 0 && isWhite(newY, newX)) Board[newY, newX] = (byte)(WHITE + promotion);
                        else if (newY == 7 && isBlack(newY, newX)) Board[newY, newX] = (byte)(BLACK + promotion);
                    }
                    else if (isKing(newY, newX))
                    {
                        if (isWhite(newY, newX)) WhiteKingPos = (newY, newX);
                        else BlackKingPos = (newY, newX);
                    }
                    if (isPawn(posY, posX) && EnPassant == (newY, newX))
                    {
                        if (isWhite(newY, newX)) Board[newY + 1, newX] = EMPTY;
                        else Board[newY - 1, newX] = EMPTY;
                    }

                    if ((isWhite(newY, newX) && ismenaced(WhiteKingPos, BLACK)) || isBlack(newY, newX) && ismenaced(BlackKingPos, WHITE)) LegalMoves.Remove((velY, velX, promotion));

                    Board[posY, posX] = temp1;
                    Board[newY, newX] = temp2;
                    if (isKing(posY, posX))
                    {
                        if (isWhite(posY, posX)) WhiteKingPos = (posY, posX);
                        else BlackKingPos = (posY, posX);
                    }
                    if (isPawn(posY, posX) && EnPassant == (newY, newX))
                    {
                        if (isWhite(posY, posX)) Board[newY + 1, newX] = BLACK + PAWN;
                        else Board[newY - 1, newX] = WHITE + PAWN;
                    }
                }
            }
            return LegalMoves;
        }

        public bool isPat(byte color)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (((color == WHITE && isWhite(y, x)) || (color == BLACK && isBlack(y, x))) && legalMoves(y, x).Count() != 0) return false;
                }
            }
            return true;
        }

        public bool move(int y1, int x1, int y2, int x2, byte promotion = EMPTY)
        {
            if (isEmpty(y1, x1) || (isWhite(y1, x1) != isWhiteTurn) || !legalMoves(y1, x1).Contains((y2 - y1, x2 - x1, promotion))) return false;
            string CurrentMove = "";
            if (isPawn(y1, x1) && (x2 - x1 == -1 || x2 - x1 == 1)) CurrentMove += (char)((byte)(97 + x1));
            else if (isRook(y1, x1)) CurrentMove += "R";
            else if (isKnight(y1, x1)) CurrentMove += "N";
            else if (isBishop(y1, x1)) CurrentMove += "B";
            else if (isQueen(y1, x1)) CurrentMove += "Q";
            else if (isKing(y1, x1)) CurrentMove += "K";
            if (!isEmpty(y2, x2)) CurrentMove += "x";
            CurrentMove += String.Format("{0}{1}", (char)((byte)(97 + x2)), 8 - y2);

            FullMoves++;
            if (isPawn(y1, x1) || !isEmpty(y2, x2)) HalfMoves = 0;
            else HalfMoves++;
            if (HalfMoves >= 50)
            {
                Winner = EMPTY;
                isfinished = true;
            }

            if (isRook(y1, x1) || isRook(y2, x2))
            {
                if ((y2, x2) == (0, 0) || (y1, x1) == (0, 0)) PossibleCasts.Remove((0, 2));
                else if ((y2, x2) == (0, 7) || (y1, x1) == (0, 7)) PossibleCasts.Remove((0, 6));
                else if ((y2, x2) == (7, 0) || (y1, x1) == (7, 0)) PossibleCasts.Remove((7, 2));
                else if ((y2, x2) == (7, 7) || (y1, x1) == (7, 7)) PossibleCasts.Remove((7, 6));
            }
            else if (isKing(y1, x1))
            {
                if (isWhite(y1, x1)) WhiteKingPos = (y2, x2);
                else BlackKingPos = (y2, x2);
                if (PossibleCasts.Contains((y2, x2)) || PossibleCasts.Contains((y1, x2)))
                {
                    if (x2 - x1 > 0)
                    {
                        CurrentMove = "0-0";
                        Board[y2, x2 - 1] = Board[y2, 7];
                        Board[y2, 7] = EMPTY;
                    }
                    else
                    {
                        CurrentMove = "0-0-0";
                        Board[y2, x2 + 1] = Board[y2, 0];
                        Board[y2, 0] = EMPTY;
                    }
                }
                PossibleCasts.Remove((y1, 2));
                PossibleCasts.Remove((y1, 6));
            }

            Board[y2, x2] = Board[y1, x1];
            Board[y1, x1] = EMPTY;
            isWhiteTurn = !isWhiteTurn;

            if (isPawn(y2, x2))
            {
                if (EnPassant == (y2, x2))
                {
                    if (isWhite(y2, x2)) Board[y2 + 1, x2] = EMPTY;
                    else Board[y2 - 1, x2] = EMPTY;
                    CurrentMove += " e.p";
                }
                else if (y2 == 0 && isWhite(y2, x2))
                {
                    Board[y2, x2] = (byte)(WHITE + promotion);
                }
                else if (y2 == 7 && isBlack(y2, x2))
                {
                    Board[y2, x2] = (byte)(BLACK + promotion);
                }

                EnPassant = (-1, -1);
                if ((y2 - y1, x2 - x1) == (2, 0) || (y2 - y1, x2 - x1) == (-2, 0))
                {
                    if (isWhite(y2, x2)) EnPassant = (y2 + 1, x2);
                    else EnPassant = (y2 - 1, x2);
                }
            }
            else EnPassant = (-1, -1);

            if (isWhite(y2, x2) && isPat(BLACK))
            {
                if (ismenaced(BlackKingPos, WHITE))
                {
                    Winner = WHITE;
                    CurrentMove += "+";
                }
                else Winner = EMPTY;
                isfinished = true;
            }
            else if (isBlack(y2, x2) && isPat(WHITE))
            {
                if (ismenaced(WhiteKingPos, BLACK))
                {
                    Winner = BLACK;
                    CurrentMove += "+";
                }
                else Winner = EMPTY;
                isfinished = true;
            }
            if (ismenaced(WhiteKingPos, BLACK) || ismenaced(BlackKingPos, WHITE)) CurrentMove += "+";

            MovesList.Add(CurrentMove);
            if (MovesList.Count >= 9 && MovesList[MovesList.Count - 1] == MovesList[MovesList.Count - 5] && MovesList[MovesList.Count - 5] == MovesList[MovesList.Count - 9] && MovesList[MovesList.Count - 3] == MovesList[MovesList.Count - 7])
            {
                if (MovesList[MovesList.Count - 2] == MovesList[MovesList.Count - 6] && MovesList[MovesList.Count - 4] == MovesList[MovesList.Count - 8])
                {
                    Winner = EMPTY;
                    isfinished = true;
                }
            }
            if (!isfinished)
            {
                isfinished = true;
                Winner = EMPTY;
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (!isEmpty(y, x) && !isKing(y, x))
                        {
                            isfinished = false;
                            Winner = 1;
                            break;
                        }
                    }
                }
                if (isfinished) CurrentMove += "++";
            }
            FEN = toFEN();
            Trace.WriteLine(String.Format("{0} ; {1}", FEN, MovesList[MovesList.Count - 1]));
            return true;
        }

        public int possibleBoards(int depth)
        {
            if (depth == 0) { return 1; }
            int count = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if ((isWhite(y, x) && isWhiteTurn) || (isBlack(y, x) && !isWhiteTurn))
                    {
                        foreach ((int velY, int velX, byte promotion) in legalMoves(y, x))
                        {
                            ChessBoard temp = new ChessBoard(FEN);
                            temp.move(y, x, y + velY, x + velX, promotion);
                            int caca = temp.possibleBoards(depth - 1);
                            count += caca;
                        }
                    }
                }
            }
            return count;
        }
    }
}