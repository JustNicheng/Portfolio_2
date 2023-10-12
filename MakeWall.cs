#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace JSNi
{
    public class MakeWall : EditorWindow
    {
        [MenuItem("Window/makeWall")]
        public static void ShowWindow()
        {

            EditorWindow.GetWindow<MakeWall>();
        }



        Vector2 scollerPos = Vector2.zero;
        private void OnGUI()
        {
            scollerPos = GUILayout.BeginScrollView(scollerPos);
            AddAllSprite();
            GUILayout.Label("牆壁生成", EditorStyles.boldLabel);
            AddBaseData();
            EditorWall();
            GUILayout.EndScrollView();
        }
        /// <summary>
        /// 城牆中心座標
        /// </summary>
        Vector3 wallPos;


        bool onlyOneWall = false;
        /// <summary>
        /// 城牆資料(總長、總厚、總高)
        /// </summary>
        Vector3Int wallStatus;
        /// <summary>
        /// 當只有一層牆壁時(OnlyOneWall)
        /// oneWallStatus = 長、高
        /// oneWallRadius = 長徑、短徑
        /// </summary>
        Vector2 oneWallStatus;
        float oneWallShortRadius;
        bool thinWall = false;
        /// <summary>
        /// 圖層層級
        /// </summary>
        int wallLayer;



        /// <summary>
        /// 道路名稱
        /// </summary>
        string _wallName;
        /// <summary>
        /// 顯示在Windows上面的內容
        /// </summary>
        void AddBaseData()
        {
            _wallName = EditorGUILayout.TextField("城牆名稱", _wallName);
            GUILayout.Space(15);
            onlyOneWall = EditorGUILayout.Toggle("單純牆壁(可以設定圓弧)", onlyOneWall);
            GUILayout.Space(15);
            wallPos = EditorGUILayout.Vector3Field("城牆中心座標", wallPos, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
            GUILayout.Space(30);
            if (onlyOneWall)
            {
                oneWallStatus = EditorGUILayout.Vector2Field("牆壁資料(長、高)", oneWallStatus, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                GUILayout.Space(15);
                thinWall = EditorGUILayout.Toggle("薄牆壁(不能曲線)", onlyOneWall);
                GUILayout.Space(15);
                if (!thinWall)
                {
                    oneWallShortRadius = EditorGUILayout.FloatField("牆壁短徑(曲線程度)", oneWallShortRadius, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    GUILayout.Space(15);
                }
            }
            else
            {
                wallStatus = EditorGUILayout.Vector3IntField("城牆資料(總長、總厚、總高)", wallStatus, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                GUILayout.Space(15);
                HaveRoad();
                GUILayout.Space(15);
                HaveParapet();
                GUILayout.Space(15);
                GUILayout.Label("圖層層級", EditorStyles.boldLabel);
                wallLayer = EditorGUILayout.IntSlider(wallLayer, -5, 0, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }
        /// <summary>
        /// 是否要有道路在牆上
        /// </summary>
        bool _haveRoad;
        /// <summary>
        /// 如果上面有路，還要分
        ///     最上層與牆壁高度差 _heightDis
        ///     寬度 _roadWidth
        ///     有幾層 _floorCount
        ///     樓層資料 _floorValue (與頂樓高度差、此樓層天花板高度、窗戶數量、窗戶高度)
        ///     兩邊窗  _twoSideWindow
        /// </summary>
        float _heightDis;
        float _roadWidth;
        /// <summary>
        /// floorCount==0 最上層
        ///           ==1 開始有內層
        ///     .
        ///     .
        ///     .
        /// </summary>
        int _floorCount;
        bool showFloorData;
        Vector4[] _floorValue = new Vector4[0];
        bool[] _LeftWindow = new bool[0];
        bool[] _RightWindow = new bool[0];
        void HaveRoad()
        {
            _haveRoad = EditorGUILayout.Toggle("是否要有道路在牆上", _haveRoad);
            if (_haveRoad)
            {
                Vector2 rWH = EditorGUILayout.Vector2Field("牆上的道路寬跟高度差", new Vector2(_roadWidth, _heightDis), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                _roadWidth = rWH.x;
                _heightDis = rWH.y;
                _floorCount = EditorGUILayout.IntField("總共幾層", _floorCount, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.Space(10);
                showFloorData = EditorGUILayout.Toggle("顯示樓層資料", showFloorData);
                if (showFloorData)
                {
                    ArrayLengthChange<Vector4>(ref _floorValue, _floorCount);
                    ArrayLengthChange<bool>(ref _LeftWindow, _floorCount);
                    ArrayLengthChange<bool>(ref _RightWindow, _floorCount);
                    //"與頂樓高度差、此樓層天花板高度、窗戶數量、窗戶高度"
                    GUILayout.Label("與頂樓高度差、此樓層天花板高度、窗戶數量、窗戶高度", EditorStyles.boldLabel);
                    for (int i = 0; i < _floorCount; i++)
                    {

                        _floorValue[i] = EditorGUILayout.Vector4Field($"{_floorCount - i}樓資料", _floorValue[i], GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                        _LeftWindow[i] = EditorGUILayout.Toggle("左邊有窗戶", _LeftWindow[i]);
                        _RightWindow[i] = EditorGUILayout.Toggle("右邊有窗戶", _RightWindow[i]);
                    }
                }
            }
        }
        /// <summary>
        /// 改變陣列長度
        /// 當陣列長度變長，新的elements會等於原本最後一筆資料
        /// 如果是從長度0開始，新的資料=default(陣列類型)
        /// 因為是泛型，所以傳進來的陣列需要ref
        /// </summary>
        /// <typeparam name="T">陣列類型</typeparam>
        /// <param name="Arr">原本陣列</param>
        /// <param name="newLen">新的陣列長度</param>
        void ArrayLengthChange<T>(ref T[] Arr, int newLen)
        {
            if (newLen != Arr.Length)
            {
                T[] newArr = new T[newLen];
                for (int i = 0; i < newLen; i++)
                {
                    newArr[i] = Arr.Length == 0 ? default(T) : Arr[i >= Arr.Length ? Arr.Length - 1 : i];
                }
                Arr = newArr;
            }
        }
        /// <summary>
        /// _haveParapet=是否像女兒牆一樣有凹凸
        /// _convexValue=女兒牆的凹凸寬、凸高
        /// </summary>
        bool _haveParapet;
        Vector2 _convexValue;
        /// <summary>
        /// 建造女兒牆
        /// 厚度=(總厚度-路寬度)/2;
        ///     =status
        /// </summary>
        void HaveParapet()
        {
            _haveParapet = EditorGUILayout.Toggle("是否要是女兒牆", _haveParapet);
            if (_haveParapet)
            {
                //原本是用凸寬，但這有可能導致最外面兩側不是凸，所以改成數量
                _convexValue = EditorGUILayout.Vector2Field("女兒牆的凸數量、凸高", _convexValue, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
            }
        }

        /// <summary>
        /// WallSprites
        /// 因為資料存取問題，所以直接連接PrefabObject
        /// </summary>
        bool[] showSpritesArray = new bool[] { true, true, true, true };
        int[] SpritesLength = new int[] { 0, 0, 0, 0 };
        Sprite[] spriteWallNew = new Sprite[0];
        Sprite[] spriteWallOld = new Sprite[0];
        Sprite[] spriteLand = new Sprite[0];
        Sprite[] spriteCeil = new Sprite[0];
        Sprite[] _nowSpritesLink;
        GameObject LinkData;
        void LinkSpriteData()
        {
            LinkData = (GameObject)EditorGUILayout.ObjectField("連結資料", LinkData, typeof(GameObject), true);
            if (GUILayout.Button("LinkData"))
            {
                MakewallData _data = LinkData.GetComponent<MakewallData>();
                spriteWallNew = _data.spriteWallNew;
                spriteWallOld = _data.spriteWallOld;
                spriteLand = _data.spriteLand;
                spriteCeil = _data.spriteCeil;
                SpritesLength = new int[] { spriteWallNew.Length, spriteWallOld.Length, spriteLand.Length, spriteCeil.Length };
                showSpritesArray = new bool[] { true, true, true, true };
            }
        }
        /// <summary>
        /// 將Sprites加進Window
        /// </summary>
        void AddAllSprite()
        {
            LinkSpriteData();
            AddSprite("新牆壁", spriteWallNew, 0);
            AddSprite("舊牆壁", spriteWallOld, 1);
            AddSprite("地板", spriteLand, 2);
            AddSprite("天花板", spriteCeil, 3);
            _nowSpritesLink = spriteWallOld;
        }
        void AddSprite(string _name, Sprite[] sprites, int spritesIndex)
        {

            SpritesLength[spritesIndex] = EditorGUILayout.IntField(_name, SpritesLength[spritesIndex], GUILayout.Width(EditorGUIUtility.currentViewWidth));

            int len = SpritesLength[spritesIndex];
            ArrayLengthChange<Sprite>(ref sprites, len);
            showSpritesArray[spritesIndex] = EditorGUILayout.Toggle("顯示陣列", showSpritesArray[spritesIndex], GUILayout.Width(EditorGUIUtility.currentViewWidth / 3 * 2));
            if (showSpritesArray[spritesIndex])
            {
                for (int i = 0; i < len; i++)
                {
                    sprites[i] = EditorGUILayout.ObjectField($"{i}:", sprites[i], typeof(Sprite), GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite;
                }
            }
            GUILayout.Space(30);
            //spriteWallNew = EditorGUILayout.ObjectField(spriteWallNew, typeof(Sprite[]), GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite[];
        }
        void EditorWall()
        {
            GUILayout.Space(20);
            if (GUILayout.Button("makeRoad"))
            {
                GameObject TheWall = new GameObject(_wallName);
                if (onlyOneWall)
                {
                    if (thinWall)
                    {
                        MakeOneWall(TheWall.transform, 0.062f);
                    }
                    else
                    {
                        MakeOneWall(TheWall.transform,1);
                    }
                }
                else
                {
                    MakeParapet(TheWall.transform);
                    MakeWallTwoSides(TheWall.transform);
                }
                TheWall.transform.position = wallPos;
            }
        }
        //0.031
        void MakeOneWall(Transform parent, float thickness)
        {
            int realWidth = Mathf.CeilToInt(oneWallStatus.x);
            float hideAngle = 0;
            if (oneWallShortRadius > 0)
            {
                hideAngle = 1 / oneWallShortRadius;//Pi / 半圓弧
                //外環
                float CenterWidth = oneWallStatus.x - oneWallShortRadius * 2;
                realWidth = Mathf.CeilToInt(Mathf.PI * oneWallShortRadius + CenterWidth);

            }
            float AngleChangeLengValue = (Mathf.Tan(hideAngle) * 0.5f + 0.5f);
            int hideAngleLimit = Mathf.CeilToInt(Mathf.PI * oneWallShortRadius / 2.0f);
            Vector3 thePos = new Vector3(.5f + 0.5f / Mathf.Cos(hideAngle) - AngleChangeLengValue * Mathf.Sin(hideAngle), .0f, .5f - AngleChangeLengValue * Mathf.Cos(hideAngle));
            int straightWidth = Mathf.CeilToInt(realWidth / 2.0f) - hideAngleLimit;
            GameObject[] lastGameObject = new GameObject[] { new GameObject("Left"), new GameObject("Right") };
            GameObject wallCenterStraight = new GameObject("WallCenter");
            wallCenterStraight.transform.parent = parent;
            wallCenterStraight.transform.localPosition = Vector3.zero;
            //1~n
            //0
            for (int i = 0; i < 2; i++)
            {
                lastGameObject[i].transform.parent = parent;
                lastGameObject[i].transform.localPosition = Mathf.Sign(i - 1) * Vector3.right * straightWidth;
            }
            float firstHeight = BaseFirstValue(oneWallStatus.y);
            for (int x = straightWidth + 1; x <= Mathf.Ceil(realWidth / 2.0f); x++)
            {

                for (int j = (x == 0) ? 1 : -1; j <= 1; j += 2)
                {
                    GameObject oneWall = new GameObject($"test({x * j})");
                    //j <= 0 -> 0
                    //j > 0  -> 1
                    int lastObjectIndex = (j + 1) / 2;
                    for (int i = -1; i <= 1; i += 2)
                    {
                        MakeManyWalls(new Vector3(.0f, Mathf.Ceil(firstHeight) / 2.0f, .0f), new Vector3(1.0f, oneWallStatus.y, .0f), new Vector3(1f, firstHeight, .0f), new Vector3(.0f, -oneWallStatus.y, i) / 2.0f, new Vector3(.0f, .0f, .0f), new int[] { 0, 1, 2 }, oneWall.transform);
                        WallSprite(new Vector3(.0f, i * oneWallStatus.y / 2.0f, .0f), new Vector4(90, .0f, .0f, 0), oneWall.transform);//上下
                    }

                    OneWallLocalTransform(x, j, hideAngle, thePos, ref oneWall, ref lastGameObject[lastObjectIndex], parent);

                }
            }
            straightWidth *= 2;
            float firstWidth = BaseFirstValue(straightWidth);
            for (int i = -1; i <= 1; i += 2)
            {
                int _index = (i + 1) / 2;
                //正面背面
                MakeManyWalls(new Vector3(Mathf.Ceil(firstWidth) / 2.0f, Mathf.Ceil(firstHeight) / 2.0f, 0), new Vector3(straightWidth, oneWallStatus.y, 0), new Vector3(firstWidth, firstHeight, 0), new Vector3(-straightWidth, -oneWallStatus.y, i*thickness) / 2.0f, Vector3.zero, new int[] { 0, 1, 2 }, wallCenterStraight.transform);
                //上下
                _nowSpritesLink = spriteWallNew;
                MakeManyWalls(new Vector3(Mathf.Ceil(firstWidth) / 2.0f, 0, 0.5f), new Vector3(straightWidth, 0, 1), new Vector3(firstWidth, 0, 1), new Vector3(-straightWidth, -oneWallStatus.y * _index, 0) / 2.0f, new Vector3(90, .0f, .0f), new int[] { 0, 2, 1 }, wallCenterStraight.transform);
                //兩側
                MakeManyWalls(new Vector3(.0f, Mathf.Ceil(firstHeight) / 2.0f, .0f), new Vector3(1.0f, oneWallStatus.y, .0f), new Vector3(1f, firstHeight, .0f), new Vector3(i, -oneWallStatus.y, 0) / 2.0f, new Vector3(.0f, 90, .0f), new int[] { 0, 1, 2 }, lastGameObject[_index].transform);
            
            }
            AddCollider(parent, Vector3.zero, new Vector3(straightWidth * 2, oneWallStatus.y, 1));

        }

        void OneWallLocalTransform(int x, int j, float hideAngle, Vector3 thePos, ref GameObject oneWall, ref GameObject lastGameObject, Transform oriParent)
        {
            AddCollider(oneWall.transform, Vector3.zero, new Vector3(1, oneWallStatus.y, 1));
            oneWall.transform.parent = lastGameObject.transform;
            //x == 0 -> 0
            //x == 1 -> 0.5
            //x >= 2 -> 1
            /*float vChange = Mathf.Sign(x - 2) * (Mathf.Sign(x - 1) + 1);
            oneWall.transform.localPosition = Vector3.right * j * (vChange + Mathf.Abs(vChange) * 3) / 8.0f;
            oneWall.transform.localRotation = Quaternion.Euler(Vector3.zero);*/
            oneWall.transform.localPosition = thePos * j;
            oneWall.transform.localRotation = Quaternion.Euler(new Vector3(.0f, hideAngle * Mathf.Rad2Deg * j, .0f));
            oneWall.transform.parent = oriParent;
            lastGameObject = oneWall;
        }


        void MakeWallTwoSides(Transform parent)
        {

            GameObject TwoSide = new GameObject($"{_wallName}_TwoSide");
            for (int z = -1; z <= 1; z += 2) MakeWallSide(TwoSide.transform, z);
            if (_haveRoad)
            {
                MakeFloorWindows(TwoSide.transform);
                MakeInside(-1, TwoSide.transform).transform.localPosition += new Vector3(.0f, wallStatus.z, .0f);
            }
            TwoSide.transform.parent = parent;
            TwoSide.transform.localPosition = new Vector3(0, 0);
        }
        void MakeWallSide(Transform parent, int sideWallIndex)
        {
            string sideWallStr = sideWallIndex == 1 ? "Left" : "Right";
            GameObject sideWall = new GameObject($"{sideWallStr}_Side");
            //wallStatus(總長、總厚、總高)
            //_convexValue女兒牆的凹凸寬、凸高
            //     樓層資料 _floorValue (與頂樓高度差、此樓層天花板高度、窗戶數量、窗戶高度)
            GameObject wallBetwWindows = new GameObject($"Between_Window");
            wallBetwWindows.transform.parent = sideWall.transform;
            wallBetwWindows.transform.localPosition = new Vector3(-.5f, .0f, ((wallStatus.y - _roadWidth) / 2 + _roadWidth / 2.0f) * sideWallIndex);
            if (_floorCount == 0) WallBetwFloors(sideWallIndex, wallStatus.y / 2.0f, sideWall.transform);

            for (int i = 0; i < _floorCount; i++)
            {
                float _y = wallStatus.z - _floorValue[i].x + _floorValue[i].w;
                MakeWallBetweenTwoWindow(i, sideWallIndex, new Vector2(_y, .0f), wallBetwWindows.transform);

            }
            //兩層樓的窗戶之間的牆壁
            WallBetwFloors(sideWallIndex, wallStatus.y / 2.0f, sideWall.transform);
            sideWall.transform.parent = parent;
        }
        void WallBetwFloors(int sideWallIndex, float thickness, Transform parent)
        {
            float wallThickness = (wallStatus.y - _roadWidth) / 2.0f;
            GameObject wallBetwFloors = new GameObject($"Between_Floors");
            wallBetwFloors.transform.parent = parent;
            wallBetwFloors.transform.localPosition = new Vector3(0, .0f, thickness * sideWallIndex);
            //wallStatus(長,厚,高)
            //_floorValue(Floor~WallTop,Floor~Ceil,WindowCount,WindowHeight)
            //沒有樓層
            if (_floorCount == 0) { MakeWallBetweenDifferentFloor(new Vector2(0, wallStatus.z), 0, wallBetwFloors.transform); AddCollider(wallBetwFloors.transform, new Vector3(.0f, wallStatus.z / 2.0f, -wallThickness / 2.0f * sideWallIndex), new Vector3(wallStatus.x, wallStatus.z, wallThickness)); return; }

            for (int i = 0; i < _floorCount; i++)
            {
                wallBetwFloors.name = wallBetwFloors.name + i.ToString();
                float thisFloor = wallStatus.z - _floorValue[i].x + _floorValue[i].w - 1.5f;

                if (i == 0) { MakeWallBetweenDifferentFloor(new Vector2(thisFloor + 3.0f, wallStatus.z), 0, wallBetwFloors.transform); AddCollider(wallBetwFloors.transform, new Vector3(.0f, (thisFloor + 2.0f + wallStatus.z) / 2.0f, -wallThickness / 2.0f * sideWallIndex), new Vector3(wallStatus.x, wallStatus.z - thisFloor - 2.0f, wallThickness)); }//最上層，得多做一次

                float nextCeil = (i == _floorCount - 1) ? 0 : wallStatus.z - _floorValue[i + 1].x + _floorValue[i + 1].w + 1.5f;
                AddCollider(wallBetwFloors.transform, new Vector3(.0f, (nextCeil + thisFloor) / 2.0f, -wallThickness / 2.0f * sideWallIndex), new Vector3(wallStatus.x, thisFloor - nextCeil + 2.0f, wallThickness));
                MakeWallBetweenDifferentFloor(new Vector2(nextCeil, thisFloor), 0, wallBetwFloors.transform);//中間
            }
        }
        /// <summary>
        /// 製作磚牆
        /// otherArg有兩種情況，
        ///          otherArg.w <  0 ,SpIndex =  (otherArg.x - 1) * 3 + (otherArg.y - 1)
        ///          otherArg.w >= 0 ,SpIndex = otherArg.w
        ///                           localRotatioin = eulerAngle(new Vector3(otherArg.x, otherArg.y, otherArg.z))
        /// </summary>
        /// <param name="pos">座標</param>
        /// <param name="otherArg">數值</param>
        /// <param name="parent">父項</param>
        /// <returns></returns>
        int WallSprite(Vector3 pos, Vector4 otherArg, Transform parent)
        {

            float realY = pos.y;
            GameObject wall = new GameObject($"Wall-{(int)pos.x}-{(int)pos.y}");
            SpriteRenderer vendorRenderer = wall.AddComponent<SpriteRenderer>();
            vendorRenderer.sprite = _nowSpritesLink[WallSpIndex(otherArg)];
            vendorRenderer.sortingOrder = wallLayer;
            wall.transform.parent = parent;
            if (otherArg.w < 0)
            {
                wall.transform.localPosition = new Vector3(pos.x, realY, 0);
            }
            else
            {
                wall.transform.localPosition = pos;
                wall.transform.eulerAngles = new Vector3(otherArg.x, otherArg.y, otherArg.z);
            }
            return 1;
        }

        /// <summary>
        /// 製作女兒牆
        /// </summary>
        /// <param name="parent"></param>
        void MakeParapet(Transform parent)
        {
            GameObject theParapet = new GameObject("theParapet");
            theParapet.transform.parent = parent;
            theParapet.transform.localPosition = new Vector3(.0f, wallStatus.z, .0f);

            GameObject[] theParapetXY = new GameObject[] { new GameObject($"theParapetXY0"), new GameObject($"theParapetXY1") };
            GameObject[] theParapetXZ = new GameObject[] { new GameObject($"theParapetXZ0"), new GameObject($"theParapetXZ1") };
            GameObject[] theParapetYZ = new GameObject[] { new GameObject($"theParapetYZ0"), new GameObject($"theParapetYZ1") };
            float wallC = (wallStatus.y + _roadWidth) / 4.0f;
            for (int i = -1; i <= 1; i += 2)
            {
                int _index = (i + 1) / 2;
                theParapetXY[_index].transform.parent = theParapet.transform;
                theParapetXY[_index].transform.localPosition = new Vector3(.0f, .0f, wallC * i);

                theParapetXZ[_index].transform.parent = theParapet.transform;
                theParapetXZ[_index].transform.localPosition = new Vector3(.0f, _convexValue.y / 2.0f, wallC * i);

                theParapetYZ[_index].transform.parent = theParapet.transform;
                theParapetYZ[_index].transform.localPosition = new Vector3(.0f, _convexValue.y / 2.0f, wallC * i);
            }

            float thickness = (wallStatus.y - _roadWidth) / 2.0f;
            float oneWidth = wallStatus.x / (_convexValue.x * 2 + 1);
            Vector3 baseFirstValue = new Vector3(BaseFirstValue(oneWidth), BaseFirstValue(_convexValue.y), BaseFirstValue(thickness));
            for (int conX = 0; conX <= _convexValue.x * 2; conX++)
            {
                //conX % 2 == 1 凹
                //conX % 2 == 0 凸
                for (int i = 0; i < 2; i++)
                {
                    //xy
                    MakeParapetXY(conX, baseFirstValue, oneWidth, thickness, theParapetXY[i].transform);
                    //xz
                    if (conX % 2 == 0) MakeParapetXZ((int)Mathf.Sign(i - 1), conX, baseFirstValue, oneWidth, thickness, theParapetXZ[i].transform);
                    //yz
                    MakeParapetYZ(conX, oneWidth, thickness, theParapetYZ[i].transform);
                }

            }
            //yz
            for (int i = 0; i <= 1; i++) MakeParapetYZ((int)_convexValue.x * 2 + 1, oneWidth, thickness, theParapetYZ[i].transform);

        }
        void MakeParapetXY(int conX, Vector3 baseFirstValue, float oneWidth, float thickness, Transform parent)
        {
            Vector3 _basePos;
            Vector3 rangeStart = new Vector3(Mathf.Ceil(baseFirstValue.x) / 2.0f, .0f, .0f);
            Vector3 rangeEnd = new Vector3(oneWidth, .0f, thickness);
            Vector3 _ro = new Vector3(90, 0, 0);
            _basePos = new Vector3(oneWidth * conX - wallStatus.x / 2.0f, ((conX + 1) % 2) * _convexValue.y, 0);
            MakeManyWalls(rangeStart, rangeEnd, baseFirstValue, _basePos, _ro, new int[] { 0, 2, 1 }, parent);
        }
        void MakeParapetXZ(int i, int conX, Vector3 baseFirstValue, float oneWidth, float thickness, Transform parent)
        {
            Vector3 _ro = new Vector3(0, 0, 0);
            Vector3 rangeStart = new Vector3(Mathf.Ceil(baseFirstValue.x) / 2.0f, .0f, .0f);
            Vector3 rangeEnd = new Vector3(oneWidth, _convexValue.y, .0f);
            Vector3 _basePos = new Vector3(oneWidth * conX - wallStatus.x / 2.0f, 0, -thickness / 2.0f * i);
            MakeManyWalls(rangeStart, rangeEnd, baseFirstValue, _basePos, _ro, new int[] { 0, 1, 2 }, parent);

            if (_haveRoad)
            {
                _basePos = new Vector3(oneWidth * conX - wallStatus.x / 2.0f, 0, thickness / 2.0f * i);
                MakeManyWalls(rangeStart, rangeEnd, baseFirstValue, _basePos, _ro, new int[] { 0, 1, 2 }, parent);
            }
            AddCollider(parent, new Vector3(oneWidth * conX - wallStatus.x / 2.0f + .5f, _convexValue.y / 2.0f - .5f, .0f), new Vector3(oneWidth, _convexValue.y, thickness));
        }
        void MakeParapetYZ(int conX, float oneWidth, float thickness, Transform parent)
        {
            Vector3 rangeStart = new Vector3(0.0f, .0f, .0f);//起始0,y,z
            Vector3 rangeEnd = new Vector3(_convexValue.y, thickness, .0f);//終點0,y,z
            Vector3 _ro = new Vector3(0, 90, 0);//旋轉0,0,0s
            Vector3 baseFirstValue = new Vector3(BaseFirstValue(_convexValue.y), BaseFirstValue(thickness), 0);
            Vector3 _basePos = new Vector3(oneWidth * conX - wallStatus.x / 2.0f, .0f, .0f);
            MakeManyWalls(rangeStart, rangeEnd, baseFirstValue, _basePos, _ro, new int[] { 1, 0, 2 }, parent);
        }


        GameObject MakeInside(int nowFloor, Transform parent)
        {
            float spaceHeight = nowFloor == -1 ? _heightDis : _floorValue[nowFloor].y;
            GameObject _inside = new GameObject($"Floor{nowFloor}-Inside");
            _inside.transform.parent = parent;
            //(Mathf.Sign(nowFloor)/2.0f+1.5f)->  nowfloor < 0 return 1 else return 2 
            _inside.transform.localPosition = new Vector3(.0f, -spaceHeight / (Mathf.Sign(nowFloor) / 2.0f + 1.5f), .0f);
            MakeRoad(nowFloor, _inside.transform);

            MakeInsideWall(nowFloor, spaceHeight, _inside.transform);
            if (nowFloor >= 0) MakeInsideCeil(nowFloor, _inside.transform);
            return _inside;
        }
        /// <summary>
        /// 製作地板，並且回傳地板相對高度
        /// </summary>
        /// <param name="nowFloor"></param>
        /// <param name="parent"></param>
        /// <returns>回傳地板高度</returns>
        void MakeRoad(int nowFloor, Transform parent)
        {
            GameObject _roadCenter = new GameObject($"InsideRoad");
            _roadCenter.transform.parent = parent;
            _roadCenter.transform.localPosition = Vector3.zero;

            _nowSpritesLink = spriteLand;

            float oneWidth = BaseFirstValue(wallStatus.x);
            float oneDepth = BaseFirstValue(_roadWidth);
            MakeManyWalls(new Vector3(Mathf.Ceil(oneWidth) / 2.0f, .0f, Mathf.Ceil(oneDepth) / 2.0f), new Vector3(wallStatus.x, .0f, _roadWidth), new Vector3(oneWidth, .0f, oneDepth), new Vector3(-wallStatus.x, .0f, -_roadWidth) / 2.0f, new Vector3(90.0f, .0f, .0f), new int[] { 0, 2, 1 }, _roadCenter.transform);



            float colliderHeight = (nowFloor >= 0 && _haveRoad) ? _floorValue[nowFloor].x : _heightDis;
            if (_floorCount > nowFloor + 1) colliderHeight = _floorValue[nowFloor + 1].x - _floorValue[nowFloor + 1].y - colliderHeight; else colliderHeight = wallStatus.z - colliderHeight;
            GameObject _co = new GameObject("Collider");
            _co.transform.parent = parent;
            _co.AddComponent<BoxCollider>().size = new Vector3(wallStatus.x, colliderHeight, _roadWidth);
            _co.transform.localPosition = new Vector3(.0f, -colliderHeight / 2.0f, .0f);
        }
        void MakeInsideWall(int nowFloor, float spaceHeight, Transform parent)
        {
            float[] WindowHeights = nowFloor == -1 ? new float[] { _heightDis } : new float[] { spaceHeight - (_floorValue[nowFloor].w + 3), _floorValue[nowFloor].w - 1.0f };
            GameObject _wallCenter = new GameObject($"InsideWall");
            _wallCenter.transform.parent = parent;
            _wallCenter.transform.localPosition = new Vector3(-.5f, spaceHeight * (Mathf.Sign(nowFloor) + 1) / 4.0f, .0f);

            _nowSpritesLink = spriteWallOld;
            float oneWidth = BaseFirstValue(wallStatus.x);
            for (int i = -1; i <= 1; i += 2)
            {
                if (nowFloor >= 0) MakeWallBetweenTwoWindow(nowFloor, i, new Vector2(.0f, _roadWidth / 2.0f * i), _wallCenter.transform);

                //-------------------------------------------Window Top----------------------------------------------Window Bottom-------------------
                foreach (float thisHeight in WindowHeights)
                {
                    float oneHeight = BaseFirstValue(thisHeight);
                    if (thisHeight > 0) MakeManyWalls(new Vector3(Mathf.Ceil(oneWidth) / 2.0f + .5f, Mathf.Ceil(oneHeight) / 2.0f, .0f), new Vector3(wallStatus.x + .5f, thisHeight, .0f), new Vector3(oneWidth, oneHeight, .0f), new Vector3(-wallStatus.x / 2.0f, .0f, _roadWidth / 2.0f * i), new Vector3(0.0f, .0f, .0f), new int[] { 0, 1, 2 }, _wallCenter.transform);
                }
            }
        }
        /// <summary>
        /// 製作天花板
        /// </summary>
        /// <param name="nowFloor">現在樓層</param>
        /// <param name="floorY">地板Y座標</param>
        /// <param name="parent"></param>
        void MakeInsideCeil(int nowFloor, Transform parent)
        {
            GameObject _ceilCenter = new GameObject($"InsideCeil");
            _ceilCenter.transform.parent = parent;
            _ceilCenter.transform.localPosition = new Vector3(.0f, _floorValue[nowFloor].y, .0f);

            _nowSpritesLink = spriteCeil;

            float oneWidth = BaseFirstValue(wallStatus.x);
            float oneDepth = BaseFirstValue(_roadWidth);
            MakeManyWalls(new Vector3(Mathf.Ceil(oneWidth) / 2.0f, .0f, Mathf.Ceil(oneDepth) / 2.0f), new Vector3(wallStatus.x, .0f, _roadWidth), new Vector3(oneWidth, .0f, oneDepth), new Vector3(-wallStatus.x, .0f, -_roadWidth) / 2.0f, new Vector3(90.0f, .0f, .0f), new int[] { 0, 2, 1 }, _ceilCenter.transform);

        }

        /// <summary>
        /// 圖的長度分成3，所以計算長度第一張的長度，剩下都是3
        /// </summary>
        /// <param name="_value">總長度</param>
        /// <returns></returns>
        float BaseFirstValue(float _value)
        {
            float baseFirstValue = _value % 3;
            if (baseFirstValue == 0) baseFirstValue = 3.0f;
            return baseFirstValue;
        }
        /// <summary>
        /// 用在MakeManyWalls，先將lastValue以及thisValue更新
        /// 再計算下一次座標的變換數值
        /// 座標變換是1~2個，不變的座標因為thisValue一定是3.0f，所以回傳的數值一定大於0，這樣只會跑一次
        /// </summary>
        /// <param name="thisValue">新的Spirest的大小</param>
        /// <param name="lastValue">前一個Spirest的大小</param>
        /// <returns></returns>
        float OneTimePlusValue(ref float thisValue, ref float lastValue)
        {
            lastValue = thisValue;
            thisValue = 3.0f;
            return lastValue - (Mathf.Ceil(lastValue) - thisValue) / 2.0f;
        }
        void MakeManyWalls(Vector3 rangeStart, Vector3 rangeEnd, Vector3 baseFirstValue, Vector3 basePos, Vector3 _rotation, int[] caculateSort, Transform parent)
        {
            float thisWidth = baseFirstValue.x, lastWidth = .0f;
            for (float _x = rangeStart.x; _x < rangeEnd.x || (_x == rangeStart.x); _x += OneTimePlusValue(ref thisWidth, ref lastWidth))
            {
                float thisHeight = baseFirstValue.y, lastHeight = .0f;
                for (float _y = rangeStart.y; _y < rangeEnd.y || (_y == rangeStart.y); _y += OneTimePlusValue(ref thisHeight, ref lastHeight))
                {
                    float thisDepth = baseFirstValue.z, lastDepth = .0f;
                    for (float _z = rangeStart.z; _z < rangeEnd.z || (_z == rangeStart.z); _z += OneTimePlusValue(ref thisDepth, ref lastDepth))
                    {
                        float spIndex = SpIndex(caculateSort, new Vector3(Mathf.Ceil(thisWidth), Mathf.Ceil(thisHeight), Mathf.Ceil(thisDepth)));
                        WallSprite(new Vector3(basePos.x + _x, basePos.y + _y, basePos.z + _z), new Vector4(_rotation.x, _rotation.y, _rotation.z, spIndex), parent);
                    }
                }
            }
        }
        int SpIndex(int[] caculateSort, Vector3 baseV)
        {
            float result = 0;
            for (int i = 0; i < 3; i++)
            {
                float theValue = 0;
                switch (caculateSort[i])
                {
                    case 0:
                        theValue = baseV.x;
                        break;
                    case 1:
                        theValue = baseV.y;
                        break;
                    case 2:
                        theValue = baseV.z;
                        break;
                }
                result += (theValue - 1) * Mathf.Max(3 - i * 2, 0);
            }
            return (int)result;
        }
        void MakeFloorWindows(Transform parent)
        {
            float windowCenterZ = (wallStatus.y - _roadWidth) / 4 + _roadWidth / 2;
            float xMin = -wallStatus.x / 2, xMax = wallStatus.x / 2;
            for (int i = 0; i < _floorCount; i++)
            {
                GameObject theWindows = new GameObject($"Floor{_floorCount - i}_Windows");
                //     樓層資料 _floorValue (與頂樓高度差、此樓層天花板高度、窗戶數量、窗戶高度)
                //_floorValue[i].x
                float _y = wallStatus.z - _floorValue[i].x + _floorValue[i].w;
                float windowX_Dis = wallStatus.x / (_floorValue[i].z + 1);
                Vector3 lastPos = new Vector3(xMin - 1, .0f, .0f);
                float startZ = _LeftWindow[i] ? -1 : 1;
                float endZ = _RightWindow[i] ? 1 : -1;
                for (float _z = startZ; _z <= endZ; _z += 2)
                {
                    for (int _x = 0; _x < _floorValue[i].z; _x++)
                    {
                        MakeWindow(new Vector3((_x + 1) * windowX_Dis + xMin - 1, .0f, _z * windowCenterZ), ref lastPos, theWindows.transform);
                    }
                }
                MakeInside(i, theWindows.transform);
                _nowSpritesLink = spriteWallOld;
                theWindows.transform.localPosition = new Vector3(.0f, _y, .0f);
                theWindows.transform.parent = parent;
            }
        }
        void MakeWindow(Vector3 pos, ref Vector3 _lastPos, Transform parent)
        {
            float thickness = (wallStatus.y - _roadWidth) / 2;
            GameObject theWindow = new GameObject("theWindow");
            theWindow.transform.localPosition = pos;
            theWindow.transform.parent = parent;
            float _colW = (pos.x - 0.5f) - (_lastPos.x + .5f);
            AddCollider(theWindow.transform, new Vector3(-_colW / 2.0f - 0.5f, 0, 0), new Vector3(_colW, 1.0f, thickness));
            _lastPos = pos;
            float helfThickness = thickness / 2.0f;
            float thisZ = BaseFirstValue(thickness);
            foreach (Vector3 vec in new Vector3[] { new Vector3(0, 0.5f, -helfThickness), new Vector3(0, -0.5f, -helfThickness), new Vector3(0.5f, 0, -helfThickness), new Vector3(-0.5f, 0, -helfThickness) })
            {
                MakeManyWalls(new Vector3(.0f, .0f, Mathf.Ceil(thisZ) / 2.0f), new Vector3(.0f, .0f, thickness), new Vector3(1.0f, .0f, thisZ), vec, new Vector3(vec.y * 180f, vec.x * 180f, vec.x * 180f), new int[] { 0, 2, 1 }, theWindow.transform);
            }
            for (float i = -1; i <= 1; i += 2)
            {
                thisZ = i * thickness / 2.0f;
                for (float j = -1; j <= 1; j += 2)
                {
                    WallSprite(new Vector3(j, 0, thisZ), new Vector4(0, 0, 90, 6), theWindow.transform);
                    WallSprite(new Vector3(0, j, thisZ), new Vector4(0, 0, 90, 0), theWindow.transform);
                }
            }
        }
        int WallSpIndex(Vector4 otherArg)
        {
            if (otherArg.w >= 0) return (int)otherArg.w;
            return (int)((otherArg.x - 1) * 3 + (otherArg.y - 1));
        }
        /// <summary>
        /// 取得同一個樓層間的X座標，並且補齊窗戶間的牆壁
        /// 這樣每層的窗戶可以簡單的化成一行 3*width的牆壁，接著只需要再用MakeWallBetweenDifferentFloor補齊每一行之間的牆壁即可
        /// 缺點：不能最佳化
        /// 優點：就算各樓層的窗戶數量不同，也不會受到任何影響
        /// </summary>
        /// <param name="nowFloor">現在樓層</param>
        /// <param name="posYZ">高度、深度校正值</param>
        /// <param name="parent">父項</param>
        void MakeWallBetweenTwoWindow(int nowFloor, int sideWallIndex, Vector2 posYZ, Transform parent)
        {
            float thickness = (wallStatus.y - _roadWidth) / 2.0f;
            if ((sideWallIndex == -1 && !_LeftWindow[nowFloor]) || (sideWallIndex == 1 && !_RightWindow[nowFloor]))
            {
                float thisWidth = wallStatus.x;
                float oneWidth = BaseFirstValue(thisWidth);
                MakeManyWalls(new Vector3(Mathf.Ceil(oneWidth) / 2.0f, .0f, .0f), new Vector3(thisWidth, .0001f, .0f), new Vector3(oneWidth, 3.0f, .0f), new Vector3(-wallStatus.x / 2.0f, posYZ.x, posYZ.y), Vector3.zero, new int[] { 0, 1, 2 }, parent);

                return;
            }
            float windowX_Dis = wallStatus.x / (_floorValue[nowFloor].z + 1);
            for (int _x = 0; _x <= _floorValue[nowFloor].z; _x++)
            {
                float _xStart = _x * windowX_Dis;
                float _xEnd = _xStart + windowX_Dis + Mathf.Sign(_x - _floorValue[nowFloor].z) - 0.5f;
                _xStart = _xStart + (Mathf.Sign(_x - 1) + 1) / 2.0f + .5f;

                float thisWidth = (_xEnd - _xStart);
                float oneWidth = BaseFirstValue(thisWidth);
                MakeManyWalls(new Vector3(Mathf.Ceil(oneWidth) / 2.0f, .0f, .0f), new Vector3(thisWidth + oneWidth / 2.0f, .0001f, .0f), new Vector3(oneWidth, 3.0f, .0f), new Vector3(_xStart - wallStatus.x / 2.0f, posYZ.x, posYZ.y), Vector3.zero, new int[] { 0, 1, 2 }, parent);

            }
        }
        /// <summary>
        /// 取得兩個樓層窗戶的高度，製作這之間的牆壁
        /// </summary>
        /// <param name="yRange">高度範圍 (TopY,BottomY)</param>
        /// <param name="posZ">z座標</param>
        /// <param name="parent">父物件</param>
        void MakeWallBetweenDifferentFloor(Vector2 yRange, float posZ, Transform parent)
        {

            float oneWidth = BaseFirstValue(wallStatus.x);
            float totalHeight = (yRange.y - yRange.x);
            float oneHeight = BaseFirstValue(totalHeight);
            MakeManyWalls(new Vector3(Mathf.Ceil(oneWidth) / 2.0f, Mathf.Ceil(oneHeight) / 2.0f, .0f), new Vector3(wallStatus.x, totalHeight, .0f), new Vector3(oneWidth, oneHeight, .0f), new Vector3(-wallStatus.x / 2.0f, yRange.x, posZ), new Vector3(.0f, .0f, .0f), new int[] { 0, 1, 2 }, parent);

        }
        //新增牆壁以及地面~天花板的Collider
        void AddCollider(Transform parent, Vector3 pos, Vector3 _size)
        {
            GameObject _co = new GameObject("Collider");
            _co.transform.parent = parent;
            _co.AddComponent<BoxCollider>().size = _size;
            _co.transform.localPosition = pos;
        }
    }
}
#endif
