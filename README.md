# U3dHotUpdate
Unity3D 热更新


前提：    
	所有要更新的资源和文件都存放在Resources目录下   
	lua文件存放在与Assets同级的Lua目录下，打包时复制到Resources/Lua目录下    
	加载资源时，先检查热更目录是否有资源，有就加载热更新目录下的，没有就加载打包的Resources目录

资源发布流程(Android)：    
	Game/AssetBundle/Build(Android)    
		打包对应平台的AssetBundle    
	Game/AssetBundle/Update(Android)    
		查找与上一个版本有差异的文件并打包成zip放到web目录(当前版本AssetBundle文件必须打包)    
		写入版本信息存到web目录    

	发布当前版本的包    

	增加版本号    
		把Config.cs VersionInfo类的ver_res_android版本号增加    
	

热更流程：
	向服务端请求最新版本号(版本号要递增并要处理多个版本的更新)
		有更新：
			下载新版本的压缩包(当前版本+1)并解压
			写入新版本号
