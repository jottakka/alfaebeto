using System; // For ArgumentNullException
using Godot;

namespace AlfaEBetto.Extensions; // Corrected namespace

/// <summary>
/// Provides extension methods for Godot's Node class.
/// </summary>
public static class NodeExtensions
{
	/// <summary>
	/// Safely adds a child node to the parent using CallDeferred("add_child", child).
	/// This is recommended when adding nodes from signal handlers or physics process
	/// to avoid potential issues with modifying the scene tree while it's being processed.
	/// </summary>
	/// <param name="parent">The parent node to add the child to.</param>
	/// <param name="child">The child node to add.</param>
	/// <exception cref="ArgumentNullException">Thrown if parent or child is null.</exception>
	public static void AddChildDefered(this Node parent, Node child) // Corrected typo: Deffered -> Deferred
	{
		// --- Input Validation ---
		if (parent == null)
		{
			// Use ArgumentNullException for invalid parameters
			throw new ArgumentNullException(nameof(parent), "Parent node cannot be null.");
			// Alternatively, log an error and return if preferred:
			// GD.PrintErr("AddChildDeferred extension called on a null parent Node.");
			// return;
		}

		if (child == null)
		{
			throw new ArgumentNullException(nameof(child), "Child node cannot be null.");
			// Alternatively:
			// GD.PrintErr("AddChildDeferred extension called with a null child Node.");
			// return;
		}
		// Optional: Check if parent is valid instance if needed
		// if (!GodotObject.IsInstanceValid(parent)) { ... }

		// --- Deferred Call ---
		// Use MethodName constants for safety against typos
		parent.CallDeferred(Node.MethodName.AddChild, child);

		// Discarding the return value with '_' is fine as CallDeferred returns a Variant we don't need here.
	}
}