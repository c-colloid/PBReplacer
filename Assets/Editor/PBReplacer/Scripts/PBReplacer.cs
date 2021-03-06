using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using VRC.Dynamics;

public class PBReplacer : EditorWindow
{
#region Variables
	private TemplateContainer _root;
	private VRCPhysBone[] _pbscript;
	private VRCPhysBoneCollider[] _pbcscripts;
	private GameObject _vrcavatar;
	private GameObject _avatarDynamicsPrefab;
	private GameObject _obj;
	private VRC_AvatarDescriptor _evt;
	private List<VRCPhysBoneColliderBase> _colliders;
#endregion

#region Methods/Unity Methods
	//メニュー追加
	[MenuItem("Tools/PBReplacer")][MenuItem("GameObject/PBReplacer",false,25)]
	public static void ShowWindow()
	{
		//ウィンドウ作成
		var wnd = GetWindow<PBReplacer>();
		//ウィンドウのタイトル
		wnd.titleContent = new GUIContent("PBReplacer");
		
		//wnd.minSize = new Vector2(600,400);
		//wnd.position = new Rect(0,0,0,0);
	}
	
	private void OnEnable()
	{
		//初期化
		_vrcavatar = null;
	}
	
	private void CreateGUI()
	{
		//UXMLからツリー構造を読み取り
		var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PBReplacer/Scripts/PBReplacer.uxml");
		var root = tree.CloneTree();
		_root = root;
		
		//ApplyButtonが押された場合
		root.Query<Button>("ApplyButton").First().clicked += () => {
			OnClickApplyBtn();
		};
		//ReloadButtonが押された場合
		root.Query<Button>("ReloadButton").First().clicked += () => {
			loadList();
		};
		
		//オブジェクトフィールドのタイプを設定
		root.Query<ObjectField>("AvatarFiled").First().objectType = typeof(VRC_AvatarDescriptor);
		
		//オブジェクトフィールドが更新された場合
		root.Q<ObjectField>("AvatarFiled").RegisterValueChangedCallback(evt =>
		{
			_evt = evt.newValue as VRC_AvatarDescriptor;
			root = loadList();
		});
		
		rootVisualElement.Add(root);
	}
#endregion

#region Methods/Other Methods
	//ApplyButtonがクリックされた場合
	private void OnClickApplyBtn()
	{
		//Debug.Log("ボタンが押されたよ");
		_avatarDynamicsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/PBReplacer/Resources/AvatarDynamics.prefab");
		if (_vrcavatar == null)
		{
			return;
		}
		if (_vrcavatar.transform.Find("AvatarDynamics") != null)
		{
			_root.Query<Label>("ToolBarLabel").First().text = "既に実行しています。他のアバターを使用してください。";
			return;
		}
		
		PlacePrefab();
		ReplacePBC();
		ReplacePB();
		
		
		_root.Query<Label>("ToolBarLabel").First().text = "完了!!!";
	}
	
	//リスト表示
	private TemplateContainer loadList()
	{
		ResetList();
	
		var vrcavatar = _evt;
		if (vrcavatar != null)
		{
			_vrcavatar = vrcavatar.gameObject;
			//Debug.Log("Avatarをセットしたよ"+_vrcavatar);
			_root.Query<Label>("ToolBarLabel").First().text = "Applyを押してください";
			_pbscript = vrcavatar.GetComponentsInChildren<VRCPhysBone>(true);
			_pbcscripts = vrcavatar.GetComponentsInChildren<VRCPhysBoneCollider>(true);
			
			foreach (var item in _pbscript)
			{
				var _newLineLabel = new Label(item.name);
				_newLineLabel.AddToClassList("myitem");
				var list = _root.Query<Foldout>("PBList").First();
				list.Add(_newLineLabel);
			}
			foreach (var item in _pbcscripts)
			{
				var _newLineLabel = new Label(item.name);
				_newLineLabel.AddToClassList("myitem");
				var list = _root.Query<Foldout>("PBCList").First();
				list.Add(_newLineLabel);
			}
		}
		else
		{
			_vrcavatar = null;
			//Debug.Log("Avatarを外したよ");
			_root.Query<Label>("ToolBarLabel").First().text = "アバターをセットしてください";
		}
		
		return _root;
	}
	
	//リスト表示のリセット
	private void ResetList()
	{
		ListView foldout = _root.Query<ListView>("PBListField").First();
		while (foldout.Query<VisualElement>(null,"myitem").Last() != null)
		{
			var element = foldout.Query<VisualElement>(null,"myitem").Last();
			element.parent.Remove(element);
		}
	}
#endregion
	
#region Methods/replacePB Methods
	//プレファブをアバターにセット
	private void PlacePrefab()
	{
		_obj = PrefabUtility.InstantiatePrefab(_avatarDynamicsPrefab) as GameObject;
		_obj.transform.SetParent(_vrcavatar.transform);
		_obj.transform.localPosition = Vector3.zero;
		//Debug.Log("Prefabを配置");
	}
	
	//PBを再配置
	private void ReplacePB()
	{
		foreach (var item in _pbscript)
		{
			if (item.rootTransform == null)
			{
				item.rootTransform = item.transform;
			}

			GameObject obj = new GameObject(item.rootTransform.name);
			Transform physBones = _obj.transform.Find("PhysBones");
			obj.transform.SetParent(physBones);
			if (item.rootTransform != item.transform)
			{
				if (physBones.transform.Find(item.name) == null)
				{
					GameObject objParent = new GameObject(item.name);
					objParent.transform.SetParent(physBones);
					objParent.transform.localPosition = Vector3.zero;
					obj.transform.SetParent(objParent.transform);
				}
				else
				{
					obj.transform.SetParent(physBones.Find(item.name));
				}
				
			}

			obj.transform.localPosition = Vector3.zero;
			
			var newphysbone = obj.AddComponent<VRCPhysBone>();
			
			System.Type type = item.GetType();
			
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var field in fields)
			{
				field.SetValue(newphysbone, field.GetValue(item));
			}
			DestroyImmediate(item);
		}
	}
	private void ReplacePBC()
	{
		foreach (var item in _pbcscripts)
		{
			if (item.rootTransform == null)
			{
				item.rootTransform = item.transform;
			}

			GameObject obj = new GameObject(item.rootTransform.name);
			Transform physBoneColliders = _obj.transform.Find("PhysBoneColliders");
			obj.transform.SetParent(physBoneColliders);
			if (item.rootTransform != item.transform)
			{
				if (physBoneColliders.transform.Find(item.name) == null)
				{
					GameObject objParent = new GameObject(item.name);
					objParent.transform.SetParent(physBoneColliders);
					objParent.transform.localPosition = Vector3.zero;
					obj.transform.SetParent(objParent.transform);
				}
				else
				{
					obj.transform.SetParent(physBoneColliders.Find(item.name));
				}
				
			}

			obj.transform.localPosition = Vector3.zero;
			
			var newphysbonecollider = obj.AddComponent<VRCPhysBoneCollider>();
			
			System.Type type = item.GetType();
			
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var field in fields)
			{
				//if (FieldInfo.Name == "roottransform")continue;
				
				field.SetValue(newphysbonecollider, field.GetValue(item));
			}
			
			//PBのコライダーを付け替え
			foreach (var pb in _pbscript)
			{
				var index = pb.colliders.IndexOf(item);
				if (index >= 0)
				{
					pb.colliders[index] = newphysbonecollider;
				}
				
			}
			
			DestroyImmediate(item);
		}
	}
#endregion
}
