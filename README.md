# (WIP) Uma.Helper.DiscordRPC.VB
 Discord RPC Plugin for umamusume (DMM)

### How to use?
 In your own hook dll, insert this code
 
```c++
//example class
class discordRpc {
private:
    HMODULE module = NULL;
    void (*dispose)();

public:
    bool (*init)();   
    void (*set)(char* msgpack, int size, const char* url);
    void (*setScene)(int sceneId);
    void (*initDB)(const char* dbPath);

    
    discordRpc() {
        module = LoadLibraryA("plugins/Uma.Helper.DiscordRPC.dll"); //edit here
        init = (bool (*)())GetProcAddress(module, "init");
        initDB = (void (*)(const char*))GetProcAddress(module, "initDB");
        set = (void (*)(char*, int, const char*))GetProcAddress(module, "processRPC");
        setScene = (void (*)(int))GetProcAddress(module, "setSceneID");
        dispose = (void (*)())GetProcAddress(module, "disposeRPC");

    }

    ~discordRpc() {
        dispose();
        FreeLibrary(module);
    }
};

//init class
discordRpc* rpc = new discordRpc();

```

 and hook the function below from umamusume

| **Assembly**              | **Namespace**     | **Class**    | **Method**              | **NumArgs** |
|---------------------------|-------------------|--------------|-------------------------|-------------|
| LibNative.Runtime.dll     | LibNative.Sqlite3 | Query        | _Setup                  | 2           |
| umamusume.dll             | Gallop            | SceneManager | LoadScene               | 1           |
| libnative.dll (Unmanaged) |                   |              | LZ4_decompress_safe_ext | 4           |


 finally, in hooked function

```c++
void* masterDBconnection = nullptr;


//LibNative.Sqlite3.Query._Setup Hooks
void* query_ctor_hook(Il2CppObject* _instance, void* conn, Il2CppString* sql)
{
   wstring path = conn->dbPath->start_char;
   if (masterDBconnection == nullptr && hasEnding(string(path.begin(), path.end()), "master.mdb")) {
      masterDBconnection = conn;
			   wprintf(L"Set masterDBConnection Handle=%p dbPath=%s\n", conn->Handle, conn->dbPath->start_char);

      //init rpc plugin
			   rpc->initDB(string(path.begin(), path.end()).c_str());

   }
   return reinterpret_cast<decltype(query_ctor_hook)*>(query_ctor_orig)(_instance, conn, sql);	
}

//Gallop.SceneManager.LoadScene
void* Gallop_SceneManager_LoadScene_hook(Il2CppObject* _instance, int sceneId) {
		printf("LoadScene id=%d\n",sceneId);

  //Set Scene ID for Discord RPC
		rpc->setScene(sceneId);
		return reinterpret_cast<decltype(Gallop_SceneManager_LoadScene_hook)*>(Gallop_SceneManager_LoadScene_orig)(_this, sceneId);
}

//LZ4_decompress_safe_ext from libnative.dll
int LZ4_decompress_safe_ext_hook(char* src, char* dst, int compressedSize, int dstCapacity) {
  char* decrypted = NULL; //Buffer for decrypted data
  int ret = 0;

  //Delete 4 bytes from header, and copy to new array
  char* realLZ4buff = new char[compressedSize - (int)4];
		decrypted = new char[dstCapacity];
		memcpy(realLZ4buff, src + 4, compressedSize - (int)4);

  ret = LZ4_decompress_safe(realLZ4buff,decrypted,compressedSize-(int)4,dstCapacity); //Original function

  rpc->set(decrypted, ret, currentUrl.str().c_str());
		//printf("%s\n", msgPackToJson(decrypted, ret));
		delete[] decrypted;
}
```

### Todo
- Multilang Support (JP, EN)
- Add more scene
