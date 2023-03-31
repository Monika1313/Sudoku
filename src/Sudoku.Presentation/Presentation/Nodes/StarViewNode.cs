﻿namespace Sudoku.Presentation.Nodes;

/// <summary>
/// Defines a star view node.
/// </summary>
/// <param name="identifier"><inheritdoc cref="IconViewNode(Identifier, int)" path="/param[@name='identifier']"/></param>
/// <param name="cell"><inheritdoc cref="IconViewNode(Identifier, int)" path="/param[@name='cell']"/></param>
public sealed class StarViewNode(Identifier identifier, int cell) : IconViewNode(identifier, cell)
{
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override StarViewNode Clone() => new(Identifier, Cell);
}
