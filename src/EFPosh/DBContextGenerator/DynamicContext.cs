using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace DBContextGenerator
{
    public static class DynamicContext
    {
        public static Type Build(Type[] Types)
        {
            /*
             * Code adapted from
             * https://github.com/rschreiber/EFCoreDynamic/blob/master/DynamicContextBuilder.cs
             */
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()),
                AssemblyBuilderAccess.Run);

            ModuleBuilder myModBuilder =
                assemblyBuilder.DefineDynamicModule("DynamicContext");

            TypeBuilder typeBuilder = myModBuilder.DefineType("PoshContext",
                TypeAttributes.Public, typeof(DbContext));
            
            var baseConstructor = typeof(DbContext).GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[] { typeof(DbContextOptions) }, null);

            //Set up the default constructor to call base
            var defaultConstructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]);
            var defaultConstructorIl = defaultConstructor.GetILGenerator();
            defaultConstructorIl.Emit(OpCodes.Ldarg_0);                // push "this";
            defaultConstructorIl.Emit(OpCodes.Call, baseConstructor);
            //The compiler always adds two nops
            defaultConstructorIl.Emit(OpCodes.Nop);
            defaultConstructorIl.Emit(OpCodes.Nop);
            defaultConstructorIl.Emit(OpCodes.Ret);


            //Set up the construct that passes the DB Context Options - change this section here if you're calling other overloads to DB context
            var dbContextConstructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(DbContextOptions) });

            var dbContextConstructorIl = dbContextConstructor.GetILGenerator();

            dbContextConstructorIl.Emit(OpCodes.Ldarg_0);                // push "this";
            dbContextConstructorIl.Emit(OpCodes.Ldarg_1);                // push the 1st parameter
            dbContextConstructorIl.Emit(OpCodes.Call, baseConstructor);
            //The compiler always adds two nops
            dbContextConstructorIl.Emit(OpCodes.Nop);
            dbContextConstructorIl.Emit(OpCodes.Nop);
            dbContextConstructorIl.Emit(OpCodes.Ret);

            var onModelCreatingMethod = typeBuilder.DefineMethod(
                            "OnModelCreating",
                            MethodAttributes.Public | MethodAttributes.Virtual,
                            null,
                            new Type[] { typeof(ModelBuilder) }
                        );
            var onModelCreatingIlGen = onModelCreatingMethod.GetILGenerator();
            onModelCreatingIlGen.Emit(OpCodes.Call,)

            foreach (var modelType in Types)
            {
                var shortName = modelType.Name;

                //Create the generic DbSet
                Type dbSetType = typeof(DbSet<>);
                var dbSetGenericType = dbSetType.MakeGenericType(modelType);

                //Backing Field
                FieldBuilder fieldBuilder = typeBuilder.DefineField(shortName.ToLower(),
                    dbSetGenericType,
                    FieldAttributes.Private);

                //Property Builder
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(shortName,
                    PropertyAttributes.HasDefault,
                    dbSetGenericType,
                    null);

                //Getter
                MethodAttributes getSetAttr =
                    MethodAttributes.Public | MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig;

                MethodBuilder getPropBuilder =
                    typeBuilder.DefineMethod($"get_{shortName}",
                        getSetAttr,
                        typeof(string),
                        Type.EmptyTypes);

                ILGenerator getIL = getPropBuilder.GetILGenerator();

                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getIL.Emit(OpCodes.Ret);

                //Setter
                MethodBuilder setPropBuilder =
                    typeBuilder.DefineMethod($"set_{shortName}",
                        getSetAttr,
                        null,
                        new Type[] { typeof(string) });

                ILGenerator setIL = setPropBuilder.GetILGenerator();

                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);

                //Assign them
                propertyBuilder.SetGetMethod(getPropBuilder);
                propertyBuilder.SetSetMethod(setPropBuilder);
            }

            Type returnType = typeBuilder.CreateTypeInfo();

            return returnType;
        }
    }
}
