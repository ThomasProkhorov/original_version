using System;
using System.Collections.Generic;
using System.Reflection;
using Uco.Infrastructure.Livecycle;
using Uco.Infrastructure.Tasks;
using Uco.Models;
using System.Linq;

namespace Uco.Infrastructure
{
    public static partial class SF
    {
        public static void PrepareReflection()
        {
            if(!SF.UsePlugins())
            {
                List<Type> PageTypesRepository = new List<Type>();
                List<Type> AdminControlerTypesRepository = new List<Type>();
                List<ITask> TasksRepository = new List<ITask>();
                Dictionary<string, Type> PageTypesDictionaryReprository = new Dictionary<string, Type>();

                AppDomain currentDomain = AppDomain.CurrentDomain;
                Assembly assembly = currentDomain.GetAssemblies().Where(r => r.FullName.StartsWith("Uco,")).First();

                PageTypesRepository.AddRange(from t in assembly.GetTypes()
                    where t.IsSubclassOf(typeof(AbstractPage))
                    select t);

                AdminControlerTypesRepository.AddRange(from t in assembly.GetTypes()
                    where t.IsSubclassOf(typeof(BaseAdminController))
                    select t);

                var instances = from t in assembly.GetTypes() 
                    where t.GetInterfaces().Contains(typeof(ITask)) && t.GetConstructor(Type.EmptyTypes) != null
                    select Activator.CreateInstance(t) as ITask;
                if (SF.UseTasks())
                {
                    TasksRepository.AddRange(instances);
                }
                foreach (Type t in PageTypesRepository)
                {
                    System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);
                    foreach (System.Attribute attr in attrs)
                    {
                        if (attr is RouteUrlAttribute)
                        {
                            RouteUrlAttribute a = (RouteUrlAttribute)attr;
                            PageTypesDictionaryReprository.Add(a.RouteUrl, t);
                        }
                    }
                }

                LS.Cache["PageTypesRepository"] = PageTypesRepository;
                LS.Cache["AdminControlerTypesRepository"] = AdminControlerTypesRepository;
                LS.Cache["TasksRepository"] = TasksRepository;
                LS.Cache["PageTypesDictionaryReprository"] = PageTypesDictionaryReprository;
            }
            else
            {
                LS.Cache["PageTypesRepository"] = new List<Type>();;
                LS.Cache["AdminControlerTypesRepository"] = new List<Type>();;
                LS.Cache["TasksRepository"] = new List<ITask>();
                LS.Cache["PageTypesDictionaryReprository"] = new Dictionary<string, Type>();
            }


            //Dictionary<string, Type> dictionary = new Dictionary<string, Type>();

            //using (AssemblyLocator al = new AssemblyLocator())
            //{
            //    ReadOnlyCollection<Assembly> UcoAssemblies = al.GetUcoAssemblies();
            //    foreach (Assembly item1 in UcoAssemblies)
            //    {
            //        foreach (Type item2 in item1.GetTypes().Where(r => r.IsSubclassOf(typeof(AbstractPage))).ToList())
            //        {
            //            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(item2);
            //            foreach (System.Attribute attr in attrs)
            //            {
            //                if (attr is RouteUrlAttribute)
            //                {
            //                    RouteUrlAttribute a = (RouteUrlAttribute)attr;
            //                    dictionary.Add(a.RouteUrl, item2);
            //                }
            //            }
            //        }
            //    }
            //}

            //LS.Cache[Token] = dictionary;
            //return dictionary;






            //List<ITask> l = new List<ITask>();
            //foreach (var assembly in RP.GetUcoAssemblyReprository())
            //{
            //    var instances = from t in assembly.GetTypes()
            //                    where t.GetInterfaces().Contains(typeof(ITask))
            //                             && t.GetConstructor(Type.EmptyTypes) != null
            //                    select Activator.CreateInstance(t) as ITask;
            //    if (assembly.GetName().Name == "Uco" || RP.GetPluginsReprository().Contains(assembly.GetName().Name))
            //    {
            //        l.AddRange(instances);
            //    }
            //}

            //LS.Cache[Token] = l;
            //return l;


        }


        //public static List<Type> GetPlugingsEntityModels()
        //{
        //    List<Type> Types = new List<Type>();
        //    foreach (var assembly in RP.GetUcoAssemblyReprository())
        //    {
        //        if (GetPluginsReprository().Contains(assembly.GetName().Name))
        //        {
        //            var entityTypes = assembly
        //              .GetTypes()
        //              .Where(t =>
        //                t.GetCustomAttributes(typeof(AddToEntityModelAttribute), inherit: true)
        //                .Any());

        //            foreach (var type in entityTypes)
        //            {
        //                Types.Add(type);
        //            }
        //        }
        //    }

        //    return Types;
        //}

        //public static List<Type> GetPlugingsAbstractPageChildClasses()
        //{
        //    List<Type> Types = new List<Type>();

        //    foreach (var assembly in RP.GetUcoAssemblyReprository())
        //    {
        //        if(GetPluginsReprository().Contains(assembly.GetName().Name))
        //        {
        //            var PageTypes = assembly
        //              .GetTypes()
        //              .Where(t =>
        //                  t.FullName.StartsWith("Uco.")
        //                  && t.FullName.Contains(".Models.")
        //                  && t.IsSubclassOf(typeof(AbstractPage))
        //               );

        //            Types.AddRange(PageTypes);
        //        }
        //    }

        //    return Types;
        //}

        //public static List<Type> GetMainAbstractPageChildClasses()
        //{
        //    List<Type> Types = new List<Type>();

        //    foreach (var assembly in RP.GetUcoAssemblyReprository().Where(r => r.FullName.StartsWith("Uco") && r.FullName.Contains("Uco.") == false))
        //    {
        //        var PageTypes = assembly
        //          .GetTypes()
        //          .Where(t =>
        //              t.FullName.StartsWith("Uco.")
        //              && t.FullName.Contains(".Models.")
        //              && t.IsSubclassOf(typeof(AbstractPage))
        //           );

        //        Types.AddRange(PageTypes);
        //    }

        //    return Types;
        //}

        //public static List<Type> GetAbstractPageChildClasses()
        //{
        //    List<Type> Types = new List<Type>();

        //    Types.AddRange(RP.GetMainAbstractPageChildClasses());
        //    Types.AddRange(RP.GetPlugingsAbstractPageChildClasses());

        //    return Types;
        //}


   //             private readonly ReadOnlyCollection<Assembly> AllAssemblies;
   //     private readonly ReadOnlyCollection<Assembly> BinAssemblies;
   //     private readonly ReadOnlyCollection<Assembly> UcoAssemblies;

   //     public AssemblyLocator()
   //     {
   //         if (SF.UsePlugins())
   //         {
   //             AllAssemblies =
   //  new ReadOnlyCollection<Assembly>(
   //                 BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList()
   //                 );

   //         }
   //         else
   //         {
   //             AllAssemblies =
   //new ReadOnlyCollection<Assembly>(
   //               new List<Assembly>()
   //               );
   //         }
   //         IList<Assembly> binAssemblies = new List<Assembly>();
   //         IList<Assembly> pluginAssemblies = new List<Assembly>();

   //         string binFolder = HttpRuntime.AppDomainAppPath + "bin\\";
   //         IList<string> dllFiles = Directory.GetFiles(binFolder, "*.dll",
   //             SearchOption.TopDirectoryOnly).ToList();

   //         foreach (string dllFile in dllFiles)
   //         {
   //             AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllFile);

   //             Assembly locatedAssembly = AllAssemblies.FirstOrDefault(a =>
   //                 AssemblyName.ReferenceMatchesDefinition(
   //                     a.GetName(), assemblyName));

   //             if (locatedAssembly != null)
   //             {
   //                 binAssemblies.Add(locatedAssembly);
   //                 if (locatedAssembly.FullName.StartsWith("Uco"))
   //                 {
   //                     pluginAssemblies.Add(locatedAssembly);
   //                 }
   //             }
   //         }
           
   //         BinAssemblies = new ReadOnlyCollection<Assembly>(binAssemblies);
   //         UcoAssemblies = new ReadOnlyCollection<Assembly>(pluginAssemblies);
   //     }

   //     public ReadOnlyCollection<Assembly> GetAssemblies()
   //     {
   //         return AllAssemblies;
   //     }

   //     public ReadOnlyCollection<Assembly> GetBinFolderAssemblies()
   //     {
   //         return BinAssemblies;
   //     }

   //     public ReadOnlyCollection<Assembly> GetUcoAssemblies()
   //     {
   //         return UcoAssemblies;
   //     }

   //     public void Dispose()
   //     {

   //     }
    }
}