﻿//
// PropertyPadObjectEditor.cs
//
// Author:
//       jmedrano <josmed@microsoft.com>
//
// Copyright (c) 2018 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if MAC

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonoDevelop.Core;
using Xamarin.PropertyEditing;

namespace MonoDevelop.DesignerSupport
{
	class ComponentModelObjectEditor
		: IObjectEditor, INameableObject
	{
		private readonly ComponentModelTarget propertyItem;
		public string Name { get; private set; }

		static IReadOnlyList<string> defaultHandlerList = new List<string> ().AsReadOnly ();
		static AssignableTypesResult defaultAssignableTypeResult = new AssignableTypesResult (new List<ITypeInfo> ().AsReadOnly ());
		private static readonly IObjectEditor [] EmptyDirectChildren = Array.Empty<IObjectEditor> ();

		private readonly List<IPropertyInfo> properties = new List<IPropertyInfo> ();
		private readonly List<PropertyDescriptorEventInfo> events = new List<PropertyDescriptorEventInfo> ();

		public object Target => this.propertyItem;

		public ITypeInfo TargetType => ToTypeInfo (Target.GetType ());

		public static ITypeInfo ToTypeInfo (Type type, bool isRelevant = true)
		{
			var asm = type.Assembly.GetName ().Name;
			return new TypeInfo (new AssemblyInfo (asm, isRelevant), type.Namespace, type.Name);
		}

		public IReadOnlyCollection<IPropertyInfo> Properties => this.properties;

		public IReadOnlyDictionary<IPropertyInfo, KnownProperty> KnownProperties => null;

		public IObjectEditor Parent => null;

		public IReadOnlyList<IObjectEditor> DirectChildren => EmptyDirectChildren;
		public IReadOnlyCollection<IEventInfo> Events => this.events;

		internal static Task<AssignableTypesResult> GetAssignableTypes ()
			=> Task.FromResult (defaultAssignableTypeResult);

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public ComponentModelObjectEditor (ComponentModelTarget propertyItem)
		{
			this.propertyItem = propertyItem;
			this.properties.AddRange (ComponentModelEditorProvider.GetPropertiesForProviders (propertyItem.Providers));
		}

		public async Task<AssignableTypesResult> GetAssignableTypesAsync (IPropertyInfo property, bool childTypes)
		{
			return new AssignableTypesResult (Array.Empty<ITypeInfo> ());
		}

		public Task<IReadOnlyList<string>> GetHandlersAsync (IEventInfo ev) => Task.FromResult(defaultHandlerList);

		public Task<string> GetNameAsync () => Task.FromResult (Name);

		public Task<IReadOnlyCollection<PropertyVariation>> GetPropertyVariantsAsync (IPropertyInfo property)
		 => Task.FromResult<IReadOnlyCollection<PropertyVariation>> (Array.Empty<PropertyVariation> ());

		public async Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variations = null)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));

			if (!(property is DescriptorPropertyInfo propertyInfo))
				throw new ArgumentException ("Property should be a DescriptorPropertyInfo", nameof (property));

			T value = await propertyInfo.GetValueAsync<T> (this);
			return new ValueInfo<T> {
				Value = value,
				Source = ValueSource.Local,
			};
		}

		public Task RemovePropertyVariantAsync (IPropertyInfo property, PropertyVariation variant) => Task.CompletedTask;

		public Task SetNameAsync (string name)
		{
			Name = name;
			return Task.FromResult (true);
		}

		public Task SetValueAsync<T> (IPropertyInfo propertyInfo, ValueInfo<T> value, PropertyVariation variations = null)
		{
			try {
				if (propertyInfo == null)
					throw new ArgumentNullException (nameof (propertyInfo));

				if (propertyInfo is DescriptorPropertyInfo info && info.CanWrite) {

					info.SetValue (this, value.Value);
					OnPropertyChanged (info);
					
				} else {
					throw new ArgumentException ($"Property should be a writeable {nameof (DescriptorPropertyInfo)}.", nameof (propertyInfo));
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Error setting the value", ex);
			}
			return Task.CompletedTask;
		}

		protected virtual void OnPropertyChanged (IPropertyInfo property)
		{
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}
	}
}

#endif