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
        ///   Looks up a localized string similar to CancellationToken.None.
        /// </summary>
        internal static string CancellationTokenNone {
            get {
                return ResourceManager.GetString("CancellationTokenNone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to default(CancellationToken).
        /// </summary>
        internal static string DefaultCancellationToken {
            get {
                return ResourceManager.GetString("DefaultCancellationToken", resourceCulture);
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
        ///   Looks up a localized string similar to CancellationToken.None should not be explicitly provided in method calls..
        /// </summary>
        internal static string PropagateCancellationTokenDescription {
            get {
                return ResourceManager.GetString("PropagateCancellationTokenDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is explicitly provided in a &apos;{1}&apos; method call..
        /// </summary>
        internal static string PropagateCancellationTokenMessageFormat {
            get {
                return ResourceManager.GetString("PropagateCancellationTokenMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Propagate CancellationToken.
        /// </summary>
        internal static string PropagateCancellationTokenTitle {
            get {
                return ResourceManager.GetString("PropagateCancellationTokenTitle", resourceCulture);
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
