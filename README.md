# U3dHotUpdate
Unity3D hot update


资源发布流程(Android)：
	点击菜单 Game->Build AssetBundles(Android) 生成AssetBundle
	更新wwwroot/AssetBundle/Android/version版本号
	把AssetBundle目录下所有文件复制到wwwroot/AssetBundle/Android/v新版本号 目录下面
	更新Config.cs里面版本号
	

热更流程：
	热更新目录resourcesinfo文件
		有：
			使用这个资源文件
		无：
			使用安装包资源文件

	向服务端请求最新版本号(版本号要递增并要处理多个版本的更新)
		有更新：
			下载服务端对应版本resourcesinfo同旧版本resourcesinfo对比是否需要更新
			如果旧版本不存在或有变化则去服务端下载
			用新的resourcesinfo替换旧的并使用这个资源文件
			写入最新版本号
