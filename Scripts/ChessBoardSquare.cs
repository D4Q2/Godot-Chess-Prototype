/*
 * Copyright 2024, Daniel from Saber C++
 * All rights reserved.
 * 
 * This source code is licensed under the BSD-style license found in the
 * LICENSE file in the root directory of this source tree. 
 *
 * (The BSD license basically means you can do whatever you want with this code (modify it, sell it, etc.) but you can't complain to me if it doesn't work perfectly and you can't use my name in association with anything you do with this code.)
 */

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
