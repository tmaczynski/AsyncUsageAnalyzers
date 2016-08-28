﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AsyncUsageAnalyzers.Usage {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UsageResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UsageResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AsyncUsageAnalyzers.Usage.UsageResources", typeof(UsageResources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Asynchronous anonymous functions and methods.
        /// </summary>
        internal static string AsyncAnonymousFunctionsAndMethods {
            get {
                return ResourceManager.GetString("AsyncAnonymousFunctionsAndMethods", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Don&apos;t use Thread.Sleep() .
        /// </summary>
        internal static string DontUseThreadSleepDescription {
            get {
                return ResourceManager.GetString("DontUseThreadSleepDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Don&apos;t use Thread.Sleep() in a aync code.
        /// </summary>
        internal static string DontUseThreadSleepInAsyncCodeDescription {
            get {
                return ResourceManager.GetString("DontUseThreadSleepInAsyncCodeDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} should not call Thread.Sleep().
        /// </summary>
        internal static string DontUseThreadSleepInAsyncCodeMessageFormat {
            get {
                return ResourceManager.GetString("DontUseThreadSleepInAsyncCodeMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Don&apos;t use Thread.Sleep() In async code.
        /// </summary>
        internal static string DontUseThreadSleepInAsyncCodeTitle {
            get {
                return ResourceManager.GetString("DontUseThreadSleepInAsyncCodeTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Thread.Sleep() should not be used.
        /// </summary>
        internal static string DontUseThreadSleepMessageFormat {
            get {
                return ResourceManager.GetString("DontUseThreadSleepMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Don&apos;t use Thread.Sleep().
        /// </summary>
        internal static string DontUseThreadSleepTitle {
            get {
                return ResourceManager.GetString("DontUseThreadSleepTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Asynchronous methods should include a CancellationToken parameter..
        /// </summary>
        internal static string IncludeCancellationParameterDescription {
            get {
                return ResourceManager.GetString("IncludeCancellationParameterDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method &apos;{0}&apos; should include a CancellationToken parameter..
        /// </summary>
        internal static string IncludeCancellationParameterMessageFormat {
            get {
                return ResourceManager.GetString("IncludeCancellationParameterMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Include CancellationToken parameter.
        /// </summary>
        internal static string IncludeCancellationParameterTitle {
            get {
                return ResourceManager.GetString("IncludeCancellationParameterTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to lambda function.
        /// </summary>
        internal static string LambdaFunction {
            get {
                return ResourceManager.GetString("LambdaFunction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Method &apos;{0}&apos;.
        /// </summary>
        internal static string MethodFormat {
            get {
                return ResourceManager.GetString("MethodFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The continuation behavior for a Task should be configured by calling ConfigureAwait prior to awaiting the task..
        /// </summary>
        internal static string UseConfigureAwaitDescription {
            get {
                return ResourceManager.GetString("UseConfigureAwaitDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Call ConfigureAwait before awaiting a Task.
        /// </summary>
        internal static string UseConfigureAwaitMessageFormat {
            get {
                return ResourceManager.GetString("UseConfigureAwaitMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use ConfigureAwait.
        /// </summary>
        internal static string UseConfigureAwaitTitle {
            get {
                return ResourceManager.GetString("UseConfigureAwaitTitle", resourceCulture);
            }
        }
    }
}
