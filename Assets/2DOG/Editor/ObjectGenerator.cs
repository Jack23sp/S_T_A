using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Generates a set amount of objects on a specified area
///
/// Author: Johan Thallauer
/// </summary>

namespace Jsoft.ObjectGenerator
{
	public class ObjectGenerator : EditorWindow
	{
		private List<String> areaBoundsList = new List<String>() {"Sprite", "BoxCollider2D"};	// Area bounds popup options
		private List<GeneratorArea> generatorAreaList = new List<GeneratorArea>(); // Area which objects will be generated within
		private List<GeneratorObject> generatorObjectList = new List<GeneratorObject>();	// List of generator objects
		private int areaAssignedAmount = 0;					// Amount of areas which has been assigned
		private List<string> layerNameList = new List<string>();  // List of available layer names
		private string parentTagName = "2d_object_generator_parent"; // tag name of parent object
		Vector2 scrollPosition = Vector2.zero; // scrollbar position

		[MenuItem("Window/2D Object Generator")] // Add menu item named "2D Item Placer" to the Window menu
		public static void ShowWindow()
		{
			// Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(ObjectGenerator));
		}

		void OnGUI()
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width),  GUILayout.Height(position.height));
			foreach (SortingLayer layer in SortingLayer.layers)
			{
				layerNameList.Add(layer.name);
			}

			// count assigned area slots
			int tempCounter = 0;

			foreach (GeneratorArea area in generatorAreaList)
			{
				if (area.Item != null && area.Size != null)
				{
					tempCounter++;
				}
			}

			areaAssignedAmount = tempCounter;
			
			GUILayout.Label("2D Object Generator", EditorStyles.boldLabel); // Widnow title

			// Generator area
			EditorGUILayout.LabelField("Generator Area");
			for (int i = 0; i < generatorAreaList.Count; i++)
			{
				UnityEngine.Object generatorAreaObject = null;
				int bounds = generatorAreaList[i].Bounds;

				if (generatorAreaList[i].Item != null)
				{
					generatorAreaObject = generatorAreaList[i].Item;
				}
				else
				{
					generatorAreaObject = null;
				}

				EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 40;
					generatorAreaObject = EditorGUILayout.ObjectField("Area", generatorAreaObject, typeof(UnityEngine.Object), true, GUILayout.Width(position.width/2));
					EditorGUIUtility.labelWidth = 0;

					generatorAreaList[i].Item = generatorAreaObject;
					
					bounds = EditorGUILayout.Popup(bounds, areaBoundsList.ToArray());
					generatorAreaList[i].Bounds = bounds;

					if (generatorAreaList[i].Item != null)
					{
						if (areaBoundsList[bounds] == "BoxCollider2D") {
							GameObject areaItem = (GameObject) generatorAreaList[i].Item;
							BoxCollider2D objectBoxCollider = areaItem.GetComponent<BoxCollider2D>();

							Offsets offsetValues = new Offsets();
							offsetValues.x = objectBoxCollider.offset.x;
							offsetValues.y = objectBoxCollider.offset.y;

							generatorAreaList[i].Offsets = offsetValues;
						}
						else
						{
							Offsets offsetValues = new Offsets();
							offsetValues.x = 0;
							offsetValues.y = 0;

							generatorAreaList[i].Offsets = offsetValues;
						}
					}
					
					if (generatorAreaObject != null)
					{
						GameObject gameObjectAreaObject = (GameObject) generatorAreaObject;
						Nullable<Vector2> areaSize = GetObjectSize(gameObjectAreaObject, areaBoundsList[bounds]);

						if (generatorAreaList[i].Size == null || generatorAreaList[i].Size != areaSize)
						{
							generatorAreaList[i].Size = areaSize;
						}
						
						generatorAreaList[i].Position = GetObjectPosition(gameObjectAreaObject);
					}

					if (GUILayout.Button("Remove"))
					{
						generatorAreaList.RemoveAt(i);
					}
				EditorGUILayout.EndHorizontal();
			}

			if (GUILayout.Button("Add"))
			{
				generatorAreaList.Add(new GeneratorArea());
			}

			EditorGUILayout.Space();

			// Generator object
			EditorGUILayout.LabelField("Generator Object");

			for (int i = 0; i < generatorObjectList.Count; i++)
			{
				UnityEngine.Object generationObject = null;
				int objectLayer = generatorObjectList[i].Layer;
				int objectLayerOrder = generatorObjectList[i].LayerOrder;
				int generationAmount = generatorObjectList[i].Amount;
				bool spread = generatorObjectList[i].Spread || false;
				bool grid = generatorObjectList[i].Grid || false;
				int bounds = generatorObjectList[i].Bounds;

				if (generatorObjectList[i].Item != null)
				{
					generationObject = generatorObjectList[i].Item;
				}

				EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 40;
					generationObject = EditorGUILayout.ObjectField("Item", generationObject, typeof(UnityEngine.Object), false);
					EditorGUIUtility.labelWidth = 0;

					generatorObjectList[i].Item = generationObject;
					bounds = EditorGUILayout.Popup(bounds, areaBoundsList.ToArray(), GUILayout.Width(position.width/4));
				EditorGUILayout.EndHorizontal();

				// Layer options
				EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Object layer");
					objectLayer = EditorGUILayout.Popup(objectLayer, layerNameList.ToArray());
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
					objectLayerOrder = EditorGUILayout.IntField("Starting order in layer", objectLayerOrder);

					EditorGUIUtility.labelWidth = 50;
					generationAmount = EditorGUILayout.IntField("Amount", generationAmount);
					EditorGUIUtility.labelWidth = 0;
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 80;
					grid = EditorGUILayout.Toggle("Place in grid", grid);
					EditorGUIUtility.labelWidth = 0;

					if (areaAssignedAmount > 1)
					{
						EditorGUIUtility.labelWidth = 50;
						spread = EditorGUILayout.Toggle("Spread", spread);
						EditorGUIUtility.labelWidth = 0;
					}
				EditorGUILayout.EndHorizontal();

				if (generationObject != null)
				{
					try {
						generatorObjectList[i].Size = GetObjectSize((GameObject) generationObject, areaBoundsList[bounds]);
					}
					catch (InvalidCastException)
					{
						generatorObjectList[i].Item = null;
						Debug.LogError("Invalid Generator Object. See documentation for the requirements of the Generator Objects.");
					}
				}

				generatorObjectList[i].Layer = objectLayer;
				generatorObjectList[i].LayerOrder = objectLayerOrder;
				generatorObjectList[i].Amount = generationAmount;
				generatorObjectList[i].Spread = spread;
				generatorObjectList[i].Grid = grid;
				generatorObjectList[i].Bounds = bounds;

				if (GUILayout.Button("Remove"))
				{
					generatorObjectList.RemoveAt(i);
				}

				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
			}
			
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add"))
			{
				generatorObjectList.Add(new GeneratorObject());
			}
			
			EditorGUILayout.EndHorizontal();

			// Generation button
			if (GUILayout.Button("Generate"))
			{
				if (areaAssignedAmount == 0)
				{
					Debug.LogError("No Generator Area has been selected, add at least one Generator Area object and try again");
				}
				else
				{
					foreach (GeneratorArea area in generatorAreaList)
					{
						if(area.Item != null)
						{
							if (area.Size != null)
							{
								GameObject generationAreaGameObject = (GameObject) area.Item;

								foreach (GeneratorObject genObj in generatorObjectList)
								{
									if (genObj.Amount > 0)
									{
										if (genObj.Size != null)
										{
											createParentTag(); // create parent tag if not exist

											string parentName = "2D Object Generator"; // decide parent gameobject name
											int tagExistanceLength = GameObject.FindGameObjectsWithTag(parentTagName).Length; // count existing parents

											if (tagExistanceLength > 0)
											{
												parentName = "2D Object Generator (" + tagExistanceLength + ")";
											}

											GameObject parent = new GameObject(parentName); // create parent
											parent.tag = parentTagName; // set parent tag
											parent.AddComponent<SpriteRenderer>();
											parent.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID(layerNameList[genObj.Layer]);
											parent.GetComponent<SpriteRenderer>().sortingOrder = genObj.LayerOrder;

											// position parent on center of area to instantiate objects on
											parent.transform.position = new Vector3(
												generationAreaGameObject.transform.position.x,
												generationAreaGameObject.transform.position.y,
												generationAreaGameObject.transform.position.z
											);

											genObj.Position = GetObjectPosition((GameObject) area.Item);

											// start generating objects
											if (genObj.Grid)
											{
												GridGeneration(parent, genObj, area);
											}
											else
											{
												RandomGeneration(parent, genObj, area);
											}
										} else {
											Debug.LogError("The selected Generator Object item does not have a valid size, see the docs for more information about the requirements of the Generator Object item");
										}
									} else {
										Debug.LogError("You must generate at least one of the selected Generator Object");
									}
								}
							} else {
								Debug.LogError("The selected Generator Area does not have a valid size, see the docs for more information about the requirements of the Generator Area");
							}
						} else {
							Debug.LogError("No Generator Area has been selected, add at least one Generator Object item and try again");
						}
					}

					ObjectSorting();
				}
			}
			
			GUILayout.EndScrollView();
		}

		/// <summary>
		/// Find objects position in world
		/// </summary>
		/// <param name="objectPosition">Object to get position of</param>
		/// <returns>Position of object</returns>
		private Vector2 GetObjectPosition(GameObject objectPosition)
		{
			return objectPosition.transform.position;
		}

		/// <summary>
		/// Get size based on objects sprite renderers bounds
		/// </summary>
		/// <param name="objectSize">Object to get size of</param>
		/// <returns>Size of objects sprite editors outer bounds</returns>
		private Nullable<Vector2> GetObjectSize(GameObject objectSize, String bounds)
		{
			Nullable<Vector2> result = null;
			
			switch (bounds)
			{
				case "Sprite":
					try {
						SpriteRenderer objectSpriteRenderer = objectSize.GetComponent<SpriteRenderer>();

						result = objectSpriteRenderer.bounds.size;
					}
					catch (Exception e) {
						Debug.LogError(e);
					}
					
					break;
				case "BoxCollider2D":
					try
					{
						BoxCollider2D objectBoxCollider = objectSize.GetComponent<BoxCollider2D>();

						result = new Vector2(objectBoxCollider.size.x * objectSize.transform.localScale.x,
																 objectBoxCollider.size.y * objectSize.transform.localScale.y);
					}
					catch (Exception e) {
						Debug.LogError(e);
					}

					break;
				default:
					Debug.LogError("GetObjectSize: Unrecognized bounds option: " + objectSize);

					break;
			}

			return result;
		}

		/// <summary>
		/// Instantiate given object
		/// </summary>
		/// <param name="generationObject">Object to instantiate</param>
		/// <param name="position">Position to instantiate object on</param>
		private int InstantiateObject(GeneratorObject generationObject, Vector3 position, GameObject parent)
		{
			GameObject instantiatedObject = Instantiate((GameObject) generationObject.Item, position, Quaternion.identity);
			instantiatedObject.transform.SetParent(parent.transform, true);
			instantiatedObject.GetComponent<SpriteRenderer>().sortingOrder = generationObject.LayerOrder;
			instantiatedObject.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID(layerNameList[generationObject.Layer]);
			instantiatedObject.AddComponent<ObjectGeneratorSortingBounds>();
			instantiatedObject.GetComponent<ObjectGeneratorSortingBounds>().SetBounds(generationObject.Bounds);

			return 1;
		}

		/// <summary>
		/// Generates objects in a grid on selected area
		/// </summary>
		private void GridGeneration(GameObject parent, GeneratorObject generationObject, GeneratorArea generatorArea)
		{
			Vector3 position = new Vector3(0f, 0f, 0f);
			Vector2 generatorAreaSize = (Vector2) generatorArea.Size;
			Vector2 generatorObjectSize = (Vector2) generationObject.Size;

			float areaXpositions = Mathf.Floor(generatorAreaSize.x / generatorObjectSize.x); // how many times the object fits on the y axis
			float areaYpositions = Mathf.Floor(generatorAreaSize.y / generatorObjectSize.y); // how many times the object fits on the x axis

			int spawnedObjects = 0;
			int amount = 0;
			
			if (generationObject.Spread)
			{
				amount = (int) Mathf.Floor(generationObject.Amount / areaAssignedAmount); // apply spread
			}
			else
			{
				amount = generationObject.Amount;
			}

			for (int i = 0; i < areaYpositions; i++) // y axis, start upper
			{
				position.y = (generatorArea.Position.y + (generatorAreaSize.y / 2) - (generatorObjectSize.y / 2) - (generatorObjectSize.y * i))  + generatorArea.Offsets.y;

				for (int j = 0; j < areaXpositions; j++) // x axis, start in left
				{
					position.x = (generatorArea.Position.x - (generatorAreaSize.x / 2) + (generatorObjectSize.x / 2) + (generatorObjectSize.x * j)) + generatorArea.Offsets.x;
					spawnedObjects += InstantiateObject(generationObject, position, parent);

					if (spawnedObjects >= amount)
					{
						break;
					}
				}

				if (spawnedObjects >= amount)
				{
					break;
				}
			}
		}

		/// <summary>
		/// Generate objects on random location within the given area
		/// </summary>
		private void RandomGeneration(GameObject parent, GeneratorObject generationObject, GeneratorArea generatorArea)
		{
			Vector2 generatorAreaSize = (Vector2) generatorArea.Size;
			Vector2 generatorObjectSize = (Vector2) generationObject.Size;

			float fromPositionX = generationObject.Position.x - (generatorObjectSize.x / 2) + (generatorAreaSize.x / 2) + generatorArea.Offsets.x;
			float toPositionX = generationObject.Position.x + (generatorObjectSize.x / 2) - (generatorAreaSize.x / 2) + generatorArea.Offsets.x;

			float fromPositionY = generationObject.Position.y - (generatorObjectSize.y / 2) + (generatorAreaSize.y / 2) + generatorArea.Offsets.y;
			float toPositionY = generationObject.Position.y + (generatorObjectSize.y / 2) - (generatorAreaSize.y / 2) + generatorArea.Offsets.y;

			float randomX;
			float randomY;
			Vector3 position = new Vector3(0f, 0f, 0f);

			int amount = 0;
			if (generationObject.Spread)
			{
				amount = (int) Mathf.Floor(generationObject.Amount / areaAssignedAmount); // apply spread
			}
			else
			{
				amount = generationObject.Amount;
			}

			for (int i = 0; i < amount; i++)
			{
				randomX = UnityEngine.Random.Range(fromPositionX, toPositionX);
				randomY = UnityEngine.Random.Range(fromPositionY, toPositionY);

				position.x = randomX;
				position.y = randomY;

                InstantiateObject(generationObject, position, parent);
            }
		}

		/// <summary>
		/// Creates the tag our parent should use
		/// if it doesn't already exist
		/// </summary>
		private void createParentTag()
		{
			// Open tag manager
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty tagsProp = tagManager.FindProperty("tags");

			// First check if it is not already present
			bool found = false;
			for (int i = 0; i < tagsProp.arraySize; i++)
			{
				SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
				if (t.stringValue.Equals(parentTagName)) { found = true; break; }
			}

			// if not found, add it
			if (!found)
			{
				tagsProp.InsertArrayElementAtIndex(0);
				SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
				n.stringValue = parentTagName;
			}

			// and to save the changes
			tagManager.ApplyModifiedProperties();
		}

		/// <summary>
		/// Get list of all generated children in the scene
		/// </summary>
		/// <returns>List of children</returns>
		private IEnumerable<GameObject> CreateChildList()
		{
			List<GameObject> children = new List<GameObject>();

			// get list of all generated objects in the scene
			foreach (GameObject parent in GameObject.FindGameObjectsWithTag(parentTagName))
			{
				foreach (Transform child in parent.transform)
				{
					children.Add(child.gameObject);
				}
			}

			IEnumerable<GameObject> sortedChildren = children.OrderByDescending(obj => areaBoundsList[obj.GetComponent<ObjectGeneratorSortingBounds>().GetBounds()] == "Sprite" ? 
				obj.transform.position.y - (obj.GetComponent<SpriteRenderer>().bounds.size.y / 2) : 
				obj.transform.position.y - (obj.GetComponent<BoxCollider2D>().size.y / 2) );

			return sortedChildren;
		}

		/// <summary>
		/// Get the lowest coordinate of an items bounds
		/// </summary>
		/// <param name="item">Item to get lowest bound from</param>
		/// <returns>The y axis of items lowest bounds</returns>
		private float GetObjectBottom(GameObject item)
		{
			float itemBottom = 0f;

			switch (areaBoundsList[item.gameObject.GetComponent<ObjectGeneratorSortingBounds>().GetBounds()])
			{
				case "Sprite":
					try
					{
						itemBottom = item.transform.position.y - (item.GetComponent<SpriteRenderer>().bounds.size.y / 2);
					}
					catch (Exception e) {
						Debug.LogError(e);
					}

					break;
				case "BoxCollider2D":
					try
					{
						itemBottom = item.transform.position.y - (item.GetComponent<BoxCollider2D>().size.y / 2);
					}
					catch (Exception e) {
						Debug.LogError(e);
					}

					break;
				default:
					Debug.LogError("GetObjectBottom: Unrecognized bounds option: " + item);

					break;
			}

			return itemBottom;
		}

		/// <summary>
		/// Sort objects on layer to place the above objects in the behind positions
		/// </summary>
		private void ObjectSorting()
		{
			IEnumerable<GameObject> sortedChildren = CreateChildList();

			foreach (GameObject child in sortedChildren)
			{
				if (child.gameObject.GetComponent<ObjectGeneratorSortingBounds>() != null)
				{
					Vector3 center = child.transform.position;
					Vector2 childSize = (Vector2) GetObjectSize(child, areaBoundsList[child.gameObject.GetComponent<ObjectGeneratorSortingBounds>().GetBounds()]);
					float childBottom = GetObjectBottom(child);

					Collider2D[] allOverlappingColliders = Physics2D.OverlapBoxAll(center, childSize, 0f);
					int startingOrder = child.transform.parent.gameObject.GetComponent<SpriteRenderer>().sortingOrder;
					int highestCollisionObject = 0;

					foreach (Collider2D collisionObject in allOverlappingColliders)
					{
						if (collisionObject.gameObject.GetComponent<ObjectGeneratorSortingBounds>() != null)
						{
							float collisionBottom = GetObjectBottom(collisionObject.gameObject);

							if (collisionBottom > childBottom &&
									collisionObject.gameObject.GetComponent<SpriteRenderer>().sortingOrder > highestCollisionObject) {
								highestCollisionObject = collisionObject.gameObject.GetComponent<SpriteRenderer>().sortingOrder;
							}
						}
					}

					child.gameObject.GetComponent<SpriteRenderer>().sortingOrder = highestCollisionObject + 1;
				}
			}
		}
	}
}

/// <summary>
/// Properties for area to generate item within
/// </summary>

namespace Jsoft.ObjectGenerator
{
	public class GeneratorArea
	{
		public UnityEngine.Object Item { get; set; }
		public Nullable<Vector2> Size { get; set; }
		public Vector2 Position { get; set; }
		public int Bounds { get; set; }
		public Offsets Offsets { get; set; }
	}
}

/// <summary>
/// Properties for item to generate
/// </summary>

namespace Jsoft.ObjectGenerator
{
	public class GeneratorObject
	{
		public UnityEngine.Object Item { get; set; }
		public Nullable<Vector2> Size { get; set; }
		public Vector2 Position { get; set; }
		public int Layer { get; set; }
		public int LayerOrder { get; set; }
		public int Amount { get; set; }
		public bool Spread { get; set; }
		public bool Grid { get; set; }
		public int Bounds { get; set; }
	}
}

/// <summary>
/// Offset values for BoxCollider2D colliders
/// </summary>

namespace Jsoft.ObjectGenerator
{
	public class Offsets
	{
		public float x { get; set; }
		public float y { get; set; }
	}
}