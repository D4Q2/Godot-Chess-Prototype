// Created by Daniel in 1985

using Godot;
using System;

public partial class ChessBoardSquare : Area3D
{
	private BoardManager boardManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boardManager = GetParent<BoardManager>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	private void OnMouseHover()
	{
		boardManager.ActiveSquareChanged(this.Name);
	}
}
