using UnityEngine;

/// <summary>
/// Generated items bounds properties
/// 
/// Author: Johan Thallauer
/// </summary>

namespace Jsoft.ObjectGenerator
{
	public class ObjectGeneratorSortingBounds : MonoBehaviour
	{
		public int Bounds;	// bounds to use when sorting

		public void SetBounds(int inBounds)
		{
			Bounds = inBounds;
		}

		public int GetBounds()
		{
			return Bounds;
		}
	}
}	