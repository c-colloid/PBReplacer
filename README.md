# PBReplacer
Version 1.1

# 概要
アバターに付いているPhysBoneコンポーネントとPhysBoneColliderコンポーネントを整理するUnity拡張です。</br>
元のコンポーネントのパラメーターを保持したまま、PhysBoneとPhysBoneColliderを1オブジェクト＝1コンポーネントに分けて再配置します。

# 使用用途
・複数コンポーネントの一括編集がしたい時。</br>
1オブジェクト＝1コンポーネントなのでHierarchy上で複数選択すると一括編集することが出来ます。</br>

・アニメーションでのオンオフを簡易化させたい時。</br>
コンポーネントがボーンについていない為、好きな場所に移動させることができます。</br>
服の子に付ければ服のオンオフアニメーションでPhysBoneも止めることができます。</br>

# 使い方
1. unitypackageをインポート</br>
2. 以下のいずれかの方法で拡張ウィンドウを表示</br>
	2.1 ツールバーのTools>PBReplacerを選択</br>
	2.2　Hierarchyで右クリックをしてPBReplacerを選択</br>
3. 開いたウィンドウにアバターをドラッグ＆ドロップ</br>
4. Applyボタンを押す</br>

# その他仕様
・RootTransformが設定されていないものは自動補完します。</br>
・オブジェクトの名称はRootTransformのオブジェクト名になります。</br>
・元のコンポーネントがRootTransformと違う場所についていた場合、「コンポーネントが付いていたオブジェクト名＞RootTransformのオブジェクト名」となるように生成されます。</br>

# 連絡先
Twitter @C_Colloid
