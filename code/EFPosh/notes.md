# Future thoughts

1. C# Compliation library takes 40mb, so keep that out of main EFPosh
    * This means the scaffolder won't be in main EFPosh - All in the .exe so cross platform
2. Have two modules so far:
    * EFPosh
    * EFPosh.Scaffold
    * Plan is Scaffold won't be used everwhere and will just make dlls for EFPosh to consume
3. Hold off on making the scaffolder - that can be phase 2
    * EFPosh work:
        * Move Linq expression converter over to EFPosh - separate library and maybe only support -like in EFPosh
        * Move code to generate DBContext from Posh classes - can be it's own library, PoshDbContext


Roadmap:
- EFPosh expose in Posh creating EFPoshEntity
- EFPosh create DbContext from EFPoshEntity classes
- Query in EFPosh


Additional thoughts:
- Could potentially have a C# method to force Posh to convert things
```
T PoshConvert<T>(T object){return object; }
```

- Want to just work with existing DbContext, no EFPoshDbContext helper if possible.
