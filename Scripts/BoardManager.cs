// Created by Daniel in 1985

using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public partial class BoardManager : Node3D
{
	[Export] private Node3D piecesHolder;
	[Export] private Camera3D whiteCamera;
	[Export] private Camera3D blackCamera;
	
	private string activeSquare;
	private string[,] chessBoard = {
		{"BRook", "BKnight", "BBishop", "BQueen", "BKing", "BBishop", "BKnight", "BRook"},
		{"BPawn", "BPawn", "BPawn", "BPawn", "BPawn", "BPawn", "BPawn", "BPawn"},
		{"", "", "", "", "", "", "", ""},
		{"", "", "", "", "", "", "", ""},
		{"", "", "", "", "", "", "", ""},
		{"", "", "", "", "", "", "", ""},
		{"WPawn", "WPawn", "WPawn", "WPawn", "WPawn", "WPawn", "WPawn", "WPawn"},
		{"WRook", "WKnight", "WBishop", "WQueen", "WKing", "WBishop", "WKnight", "WRook"}
	};
	private string clickedOnPosition = "";
	// false = not clicked on a square
	// true = have clicked on a square (with a piece) and now it's time for them to choose where to move it
	private char[] chessLetters = {'a','b','c','d','e','f','g','h'};
	
	private bool isWhiteTurn = true;
	
	private bool whiteKingMoved;
	private bool whiteKingsideRookMoved;
	private bool whiteQueensideRookMoved;
	private bool blackKingMoved;
	private bool blackKingsideRookMoved;
	private bool blackQueensideRookMoved;
	
	private string en_passant_position = "";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdatePieces();
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// 
	}
	
	public void UpdatePieces() {
		foreach (Node3D piece in piecesHolder.GetChildren()) {
			piece.QueueFree();
		}
		
		// Loop over string board array
		int rowCount = 0;
		int columnCount = 0;
		foreach (string piece in chessBoard) {
			// If it's not an empty string
			if (piece != "") {
				char side = piece[0];
				string pieceName = piece.Substring(1);
				
				// Create a new mesh for that piece
				var prefab = GD.Load<PackedScene>("res://ChessModels/" + pieceName + ".glb");
				var newPiece = prefab.Instantiate<Node3D>();
				piecesHolder.AddChild(newPiece);
				
				// Find the correct square on the actual physical board
				int chessNumber = 8 - rowCount;
				char chessLetter = chessLetters[columnCount];
				string chessBoardPosition = chessLetter.ToString() + chessNumber.ToString();
				
				// Place the mesh on that square
				newPiece.Position = this.GetNode<Area3D>(chessBoardPosition).Position;
				newPiece.Scale = new Vector3(10,10,10);
				if (side == 'B') {
					MeshInstance3D newPieceModel = (MeshInstance3D)newPiece.GetChild(0);
					newPieceModel.MaterialOverride = GD.Load<Material>("res://Shaders/ChessMaterial3D.tres");
				}
			}
			columnCount++;
			if (columnCount > 7) {
				rowCount++;
				columnCount = 0;
			}
		}
	}
	
	public void ActiveSquareChanged(string newActiveSquare) {
		if (clickedOnPosition == "") {
			if (activeSquare != null) {
				SetSquareGlow(activeSquare, false);
			}
			SetSquareGlow(newActiveSquare, true);
		}
		
		activeSquare = newActiveSquare;
	}
	
	public override void _UnhandledInput(InputEvent @event) {
		if (!(@event is InputEventMouseButton mouseButton) || !mouseButton.Pressed) {
			return;
		}
		if (activeSquare == null || activeSquare == "") {
			return;
		}
		
		string piece = ChessNotationToArrayValue(activeSquare);
		
		if (clickedOnPosition != "") {
			// Get where the piece was
			string clickedOnPiece = ChessNotationToArrayValue(clickedOnPosition);
			List<string> possiblePositions = PossiblePositions(clickedOnPiece, clickedOnPosition);
			
			if (possiblePositions.Contains(activeSquare)) {
				HandleCastling(clickedOnPiece, clickedOnPosition);
				HandleEnPassant(clickedOnPiece, activeSquare, clickedOnPosition, ChessNotationToArrayValue(activeSquare));
				SwitchTurns();
				
				// Move the peice in the array
				SetArrayValueWithChessNotation(activeSquare, clickedOnPiece);
				// Remove the old piece
				SetArrayValueWithChessNotation(clickedOnPosition, "");
				clickedOnPosition = "";
				
				PromoteAnyPawns();
				UpdatePieces();
				TurnOffAllSquares();
			}
			else if (piece != "" && clickedOnPiece != "" && piece[0] == clickedOnPiece[0]) {
				TurnOffAllSquares();
				clickedOnPosition = activeSquare;
				HighlightPossiblePositions(piece, activeSquare);
			}
		}
		else if (piece != "" && (piece[0] == 'W') == isWhiteTurn) {
			clickedOnPosition = activeSquare;
			HighlightPossiblePositions(piece, activeSquare);
		}
	}
	
	private void HandleEnPassant(string movedPiece, string currentPosition, string formerPosition, string formerPiece) {
		if (movedPiece.Substring(1) == "Pawn" && formerPiece == "" && currentPosition[0] != formerPosition[0]) {
			SetArrayValueWithChessNotation(en_passant_position, "");
		}
		
		if (movedPiece == "WPawn" && currentPosition[1] == '4' && formerPosition[1] == '2') {
			en_passant_position = currentPosition;
		}
		else if (movedPiece == "BPawn" && currentPosition[1] == '5' && formerPosition[1] == '7') {
			en_passant_position = currentPosition;
		}
		else {
			en_passant_position = "";
		}
	}
	
	private void HandleCastling(string movedPiece, string formerPosition) {
		if (movedPiece == "WKing") {
			whiteKingMoved = true;
			if (formerPosition == "e1" && activeSquare == "g1") {
				SetArrayValueWithChessNotation("h1", "");
				SetArrayValueWithChessNotation("f1", "WRook");
			}
			else if (formerPosition == "e1" && activeSquare == "c1") {
				SetArrayValueWithChessNotation("a1", "");
				SetArrayValueWithChessNotation("d1", "WRook");
			}
		}
		else if (movedPiece == "WRook" && formerPosition == "h1")
			whiteKingsideRookMoved = true;
		else if (movedPiece == "WRook" && formerPosition == "a1")
			whiteQueensideRookMoved = true;
		else if (movedPiece == "BKing") {
			blackKingMoved = true;
			if (formerPosition == "e8" && activeSquare == "g8") {
				SetArrayValueWithChessNotation("h8", "");
				SetArrayValueWithChessNotation("f8", "BRook");
			}
			else if (formerPosition == "e8" && activeSquare == "c8") {
				SetArrayValueWithChessNotation("a8", "");
				SetArrayValueWithChessNotation("d8", "BRook");
			}
		}
		else if (movedPiece == "BRook" && formerPosition == "h8")
			blackKingsideRookMoved = true;
		else if (movedPiece == "BRook" && formerPosition == "a8")
			blackQueensideRookMoved = true;
	} 
	
	private void SwitchTurns() {
		isWhiteTurn = !isWhiteTurn;
		whiteCamera.Current = isWhiteTurn;
	}
	
	private void PromoteAnyPawns() {
		for (int i = 0; i < 8; i++) {
			string currentPosition = chessLetters[i] + "8";
			if (ChessNotationToArrayValue(currentPosition) == "WPawn") {
				SetArrayValueWithChessNotation(currentPosition, "WQueen");
			}
		}
		
		for (int i = 0; i < 8; i++) {
			string currentPosition = chessLetters[i] + "1";
			if (ChessNotationToArrayValue(currentPosition) == "BPawn") {
				SetArrayValueWithChessNotation(currentPosition, "BQueen");
			}
		}
	}
	
	private void HighlightPossiblePositions(string piece, string piecePosition) {
		List<string> possiblePositions = PossiblePositions(piece, piecePosition);
		foreach (string pos in possiblePositions) {
			SetSquareGlow(pos, true);
		}
	}
	
	private void TurnOffAllSquares() {
		for (int column = 0; column <= 7; column++) {
			for (int row = 1; row <= 8; row++) {
				string notation = chessLetters[column] + row.ToString();
				SetSquareGlow(notation, false);
			}
		}
	}
	private void SetSquareGlow(string squareName, bool glowing) {
		Area3D square = this.GetNode<Area3D>(squareName);
		CsgBox3D squareVisual = square.GetNode<CsgBox3D>("ColorSquare");
		StandardMaterial3D squareMaterial = (StandardMaterial3D)squareVisual.MaterialOverride;
		squareMaterial.EmissionEnabled = glowing;
	}
	private bool IsSquareGlowing(string squareName) {
		Area3D square = this.GetNode<Area3D>(squareName);
		CsgBox3D squareVisual = square.GetNode<CsgBox3D>("ColorSquare");
		StandardMaterial3D squareMaterial = (StandardMaterial3D)squareVisual.MaterialOverride;
		return squareMaterial.EmissionEnabled;
	}
	
	private void SetArrayValueWithChessNotation(string notationPosition, string newValue) {
		bool childFound = false;
		foreach (Node child in this.GetChildren()) {
			if (child.Name == notationPosition) {
				childFound = true;
				break;
			}
		}
		if (!childFound) {
			GD.Print("Child not found!");
			return;
		}
		
		int letterIndex = ChessLetterToNumber(notationPosition[0]);
		int numberIndex = 8 - Int32.Parse(notationPosition[1].ToString());
		
		chessBoard[numberIndex, letterIndex] = newValue;
	}
	private string ChessNotationToArrayValue(string notationPosition) {
		bool childFound = false;
		foreach (Node child in this.GetChildren()) {
			if (child.Name == notationPosition) {
				childFound = true;
				break;
			}
		}
		if (!childFound) {
			return "Invalid";
		}
		
		int letterIndex = ChessLetterToNumber(notationPosition[0]);
		int numberIndex = 8 - Int32.Parse(notationPosition[1].ToString());
		
		return chessBoard[numberIndex, letterIndex];
	}
	
	private int ChessLetterToNumber(char letter) {
		switch (letter) {
			case 'a':
				return 0;
			case 'b':
				return 1;
			case 'c':
				return 2;
			case 'd':
				return 3;
			case 'e':
				return 4;
			case 'f':
				return 5;
			case 'g':
				return 6;
			case 'h':
				return 7;
			default:
				return -1;
		}
	}
	
	private List<string> PossiblePositions(string piece, string position, bool ignoreCheck = false) {
		List<string> validPositions = new();
		
		char side = piece[0];
		string pieceName = piece.Substring(1);
		
		char letter = position[0];
		int letter_as_number = ChessLetterToNumber(letter);
		int number = Int32.Parse(position[1].ToString());
		
		if (pieceName == "Pawn" && side == 'W') {
			string forwardSquare = letter.ToString() + (number + 1).ToString();
			string doubleForwardSquare = letter.ToString() + (number + 2).ToString();
			
			if (ChessNotationToArrayValue(forwardSquare) == "") {
				validPositions.Add(forwardSquare);
				if (ChessNotationToArrayValue(doubleForwardSquare) == "" && number == 2) {
					validPositions.Add(doubleForwardSquare);
				}
			}
			if (ChessLetterToNumber(letter) + 1 < chessLetters.Length) {
				string rightForwardSquare = chessLetters[ChessLetterToNumber(letter) + 1] + (number + 1).ToString();
				if (ChessNotationToArrayValue(rightForwardSquare) != "" && ChessNotationToArrayValue(rightForwardSquare)[0] == 'B') {
					validPositions.Add(rightForwardSquare);
				}
			}
			if (ChessLetterToNumber(letter) - 1 >= 0) {
				string leftForwardSquare = chessLetters[ChessLetterToNumber(letter) - 1] + (number + 1).ToString();
				if (ChessNotationToArrayValue(leftForwardSquare) != "" && ChessNotationToArrayValue(leftForwardSquare)[0] == 'B') {
					validPositions.Add(leftForwardSquare);
				}
			}
			
			if (en_passant_position != "") {
				string passant_piece = ChessNotationToArrayValue(en_passant_position);
				int passant_letter_as_number = ChessLetterToNumber(en_passant_position[0]);
				
				if (en_passant_position[1] == position[1] && Math.Abs(letter_as_number - passant_letter_as_number) == 1) {
					string move_to_position = en_passant_position[0] + (Int32.Parse(en_passant_position[1].ToString()) + 1).ToString();
					if (ChessNotationToArrayValue(move_to_position) == "") {
						validPositions.Add(move_to_position);
					}
				}
			}
 		}
		if (pieceName == "Pawn" && side == 'B') {
			string forwardSquare = letter.ToString() + (number - 1).ToString();
			string doubleForwardSquare = letter.ToString() + (number - 2).ToString();
			
			if (ChessNotationToArrayValue(forwardSquare) == "") {
				validPositions.Add(forwardSquare);
				if (ChessNotationToArrayValue(doubleForwardSquare) == "" && number == 7) {
					validPositions.Add(doubleForwardSquare);
				}
			}
			
			if (ChessLetterToNumber(letter) + 1 < chessLetters.Length) {
				string rightForwardSquare = chessLetters[ChessLetterToNumber(letter) + 1] + (number - 1).ToString();
				if (ChessNotationToArrayValue(rightForwardSquare) != "" && ChessNotationToArrayValue(rightForwardSquare)[0] == 'W') {
					validPositions.Add(rightForwardSquare);
				}
			}
			if (ChessLetterToNumber(letter) - 1 >= 0) {
				string leftForwardSquare = chessLetters[ChessLetterToNumber(letter) - 1] + (number - 1).ToString();
				if (ChessNotationToArrayValue(leftForwardSquare) != "" && ChessNotationToArrayValue(leftForwardSquare)[0] == 'W') {
					validPositions.Add(leftForwardSquare);
				}
			}
			
			if (en_passant_position != "") {
				string passant_piece = ChessNotationToArrayValue(en_passant_position);
				int passant_letter_as_number = ChessLetterToNumber(en_passant_position[0]);
				
				if (en_passant_position[1] == position[1] && Math.Abs(letter_as_number - passant_letter_as_number) == 1) {
					string move_to_position = en_passant_position[0] + (Int32.Parse(en_passant_position[1].ToString()) - 1).ToString();
					if (ChessNotationToArrayValue(move_to_position) == "") {
						validPositions.Add(move_to_position);
					}
				}
			}
 		}
		if (pieceName == "Rook" || pieceName == "Queen") {
			int movingNumber = number;
			while (movingNumber < 8) {
				movingNumber++;
				string currentNotationPosition = letter + movingNumber.ToString();
				string validPosition = "";
				
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				if (!canContinue) {
					break;
				}
			}
			
			movingNumber = number;
			while (movingNumber > 1) {
				movingNumber--;
				String currentNotationPosition = letter + movingNumber.ToString();
				string validPosition;
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				if (!canContinue) {
					break;
				}
			}
			
			int movingLetterAsNumber = ChessLetterToNumber(letter);
			while (movingLetterAsNumber < 7) {
				movingLetterAsNumber++;
				char movingLetter = chessLetters[movingLetterAsNumber];
				
				String currentNotationPosition = movingLetter + number.ToString();
				string validPosition;
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				if (!canContinue) {
					break;
				}
			}
			
			movingLetterAsNumber = ChessLetterToNumber(letter);
			while (movingLetterAsNumber > 0) {
				movingLetterAsNumber--;
				char movingLetter = chessLetters[movingLetterAsNumber];
				String currentNotationPosition = movingLetter + number.ToString();
				string validPosition;
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				if (!canContinue) {
					break;
				}
			}
		}
		if (pieceName == "Bishop" || pieceName == "Queen") {
			int movingNumber = number;
			int movingLetterAsNumber = ChessLetterToNumber(letter);
			
			while (movingNumber < 8 && movingLetterAsNumber < 7) {
				movingNumber++;
				movingLetterAsNumber++;
				
				char movingLetter = chessLetters[movingLetterAsNumber];
				String currentNotationPosition = movingLetter + movingNumber.ToString();
				
				string validPosition;
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				
				if (!canContinue) {
					break;
				}
			}
			
			movingNumber = number;
			movingLetterAsNumber = ChessLetterToNumber(letter);
			while (movingNumber < 8 && movingLetterAsNumber > 0) {
				movingNumber++;
				movingLetterAsNumber--;
				
				char movingLetter = chessLetters[movingLetterAsNumber];
				String currentNotationPosition = movingLetter + movingNumber.ToString();
				
				string validPosition;
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				
				if (!canContinue) {
					break;
				}
			}
			
			movingNumber = number;
			movingLetterAsNumber = ChessLetterToNumber(letter);
			while (movingNumber > 1 && movingLetterAsNumber < 7) {
				movingNumber--;
				movingLetterAsNumber++;
				
				char movingLetter = chessLetters[movingLetterAsNumber];
				String currentNotationPosition = movingLetter + movingNumber.ToString();
				
				string validPosition;
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				
				if (!canContinue) {
					break;
				}
			}
			
			movingNumber = number;
			movingLetterAsNumber = ChessLetterToNumber(letter);
			while (movingNumber > 1 && movingLetterAsNumber > 0) {
				movingNumber--;
				movingLetterAsNumber--;
				
				char movingLetter = chessLetters[movingLetterAsNumber];
				String currentNotationPosition = movingLetter + movingNumber.ToString();
				
				string validPosition;
				bool canContinue = CanContinueSearching(currentNotationPosition, side, out validPosition);
				validPositions.Add(validPosition);
				
				if (!canContinue) {
					break;
				}
			}
		}
		if (pieceName == "King") {
			int letterAsNumber = ChessLetterToNumber(letter);
			for (int movingNumber = number - 1; movingNumber <= number + 1; movingNumber++) {
				for (int movingLetterAsNumber = letterAsNumber - 1; movingLetterAsNumber <= letterAsNumber + 1; movingLetterAsNumber++) {
					if (movingNumber < 1 || movingNumber > 8 || movingLetterAsNumber < 0 || movingLetterAsNumber > 7) {
						continue;
					}
					
					string notationPosition = chessLetters[movingLetterAsNumber] + movingNumber.ToString();
					if (notationPosition != position) {
						string newPosition;
						CanContinueSearching(notationPosition, side, out newPosition);
						
						bool squareSafe = true;
						if (!ignoreCheck) {
							for (int row = 1; row <= 8; row++) {
								if (!squareSafe)
									break;
								
								for (int col = 0; col < 8; col++) {
									string piecePosition = chessLetters[col] + row.ToString();
									string potentialPiece = ChessNotationToArrayValue(piecePosition);
									
									if (potentialPiece != "" && potentialPiece[0] != side) {
										List<string> possiblePositions = PossiblePositions(potentialPiece, piecePosition, true);
										if (possiblePositions.Contains(newPosition)) {
											squareSafe = false;
											break;
										}
									}
								}
							}
						}
						
						if (squareSafe)
							validPositions.Add(newPosition);
					}
				}
			}
			
			if (side == 'W' && !whiteKingMoved) {
				if (!whiteKingsideRookMoved && ChessNotationToArrayValue("f1") == "" && ChessNotationToArrayValue("g1") == "") {
					validPositions.Add("g1");
				}
				if (!whiteQueensideRookMoved && ChessNotationToArrayValue("b1") == "" && ChessNotationToArrayValue("c1") == "" && ChessNotationToArrayValue("d1") == "") {
					validPositions.Add("c1");
				}
			}
			if (side == 'B' && !blackKingMoved) {
				if (!blackKingsideRookMoved && ChessNotationToArrayValue("f8") == "" && ChessNotationToArrayValue("g8") == "") {
					validPositions.Add("g8");
				}
				if (!blackQueensideRookMoved && ChessNotationToArrayValue("b8") == "" && ChessNotationToArrayValue("c8") == "" && ChessNotationToArrayValue("d8") == "") {
					validPositions.Add("c8");
				}
			}
		}
		if (pieceName == "Knight") {
			List<Vector2> possibleKnightPositions = new List<Vector2>();
			possibleKnightPositions.AddRange(KnightPositionsInDirection(ChessLetterToNumber(letter), number, 1));
			possibleKnightPositions.AddRange(KnightPositionsInDirection(ChessLetterToNumber(letter), number, -1));
			
			foreach (Vector2 pos in possibleKnightPositions) {
				if (pos.X < 0 || pos.X > 7 || pos.Y < 1 || pos.Y > 8) {
					continue;
				}
				
				string notationPosition = chessLetters[Mathf.RoundToInt(pos.X)] + Mathf.RoundToInt(pos.Y).ToString();
				string potentiallyValidPosition;
				CanContinueSearching(notationPosition, side, out potentiallyValidPosition);
				validPositions.Add(potentiallyValidPosition);
			}
			possibleKnightPositions.Clear();
			
			possibleKnightPositions.AddRange(KnightPositionsInDirection(number, ChessLetterToNumber(letter), 1));
			possibleKnightPositions.AddRange(KnightPositionsInDirection(number, ChessLetterToNumber(letter), -1));
			
			foreach (Vector2 pos in possibleKnightPositions) {
				if (pos.X < 1 || pos.X > 8 || pos.Y < 0 || pos.Y > 7) {
					continue;
				}
				
				string notationPosition = chessLetters[Mathf.RoundToInt(pos.Y)] + Mathf.RoundToInt(pos.X).ToString();
				string potentiallyValidPosition;
				CanContinueSearching(notationPosition, side, out potentiallyValidPosition);
				validPositions.Add(potentiallyValidPosition);
			}
		}
		
		while (validPositions.Contains("Invalid")) {
			validPositions.Remove("Invalid");
		}
		while (validPositions.Contains("")) {
			validPositions.Remove("");
		}
		
		return validPositions;
	}
	
	List<Vector2> KnightPositionsInDirection(int longWay, int shortWay, int multiplier) {
		List<Vector2> positions = new List<Vector2>();
		Vector2 knightPos1 = new Vector2(longWay + 2*multiplier, shortWay+1);
		Vector2 knightPos2 = new Vector2(longWay + 2*multiplier, shortWay-1);
		
		positions.Add(knightPos1);
		positions.Add(knightPos2);
		
		return positions;
	}
	
	// Checks if we can continue searching after this square (of course we can't if there is a piece on this square) and (if valid) sets validPosition to the current position.
	bool CanContinueSearching(String currentPosition, char side, out string validPosition) {
		String currentPiece = ChessNotationToArrayValue(currentPosition);
		if (currentPiece == "") {
			validPosition = currentPosition;
			return true;
		}
		else if (currentPiece[0] != side) {
			validPosition = currentPosition;
		}
		else {
			validPosition = "";
		}
		
		return false;
	}
}
