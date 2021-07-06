using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BeerApp.Core
{
	public abstract class PropertyChangeNotifier : INotifyPropertyChanged
	{
		private readonly Dictionary<string, HashSet<string>> _dependentProperties = new();
		private readonly Dictionary<string, object> _boundValues = new();
		private readonly Dictionary<string, PropertyChangedEventHandler> _propertyChangedHandlers = new();
		private readonly Dictionary<string, NotifyCollectionChangedEventHandler> _collectionChangedHandlers = new();
		private bool _initialized = false;

		protected void Initialize(params (string source, string dependency)[] relationships)
		{
			_initialized = true;
			foreach(var property in GetType().GetProperties()) {
				try {
					Set(property.GetValue(this), propertyName: property.Name);
				} catch {
					// ignore
				}
			}

			foreach(var (source, dependency) in relationships) {
				if(!_dependentProperties.TryGetValue(source, out var dependencies)) {
					dependencies = new();
					_dependentProperties.Add(source, dependencies);
				}
				_ = dependencies.Add(dependency);
			}
		}

		private void CheckInitialized()
		{
			if(!_initialized) {
				throw new InvalidOperationException("Must call Initialize before accessing managed properties.");
			}
		}

		protected T Get<T>(T defaultValue, [CallerMemberName] string propertyName = "")
		{
			CheckInitialized();
			lock(_boundValues) {
				var value = _boundValues.TryGetValue(propertyName, out var existing)
					? (T)existing
					: defaultValue;
				return value;
			}
		}

		/// <summary>
		/// Sets the value of a property.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="value">The new value.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <param name="preAction">An optional action that will be performed
		/// before the value is updated.</param>
		/// <param name="postAction">An optional action that will be performed
		/// after the value has been updated.</param>
		protected void Set<T>(
			T value,
			[CallerMemberName] string propertyName = "",
			Action? preAction = null,
			Action? postAction = null
		)
		{
			CheckInitialized();
			object? existing;
			lock(_boundValues) {
				if(!_boundValues.TryGetValue(propertyName, out existing)) {
					try {
						existing = GetType().GetProperty(propertyName).GetValue(this);
					} catch {
						// suppress problems with existing value
					}
				}
				if(existing != null && EqualityComparer<T>.Default.Equals((T)existing, value)) {
					_boundValues[propertyName] = existing;
					return;
				}
			}

			RemoveChildHandlers(propertyName, existing);

			preAction?.Invoke();
			lock(_boundValues) {
				_boundValues[propertyName] = value!;
			}

			AddChildHandlers(value, propertyName);

			OnPropertyChanged(propertyName);
			postAction?.Invoke();
		}

		private void AddChildHandlers<T>(T value, string propertyName)
		{
			if(value is INotifyCollectionChanged newCollection) {
				var collectionChangedHandler = GetCollectionChangedHandler(propertyName);
				newCollection.CollectionChanged += collectionChangedHandler;
				_collectionChangedHandlers[propertyName] = collectionChangedHandler;
			} else if(value is INotifyPropertyChanged newNotifier) {
				var propertyChangedHandler = GetPropertyChangedHandler(propertyName);
				newNotifier.PropertyChanged += propertyChangedHandler;
				_propertyChangedHandlers[propertyName] = propertyChangedHandler;
			}
		}

		private void RemoveChildHandlers(string propertyName, object? existing)
		{
			if(existing is INotifyCollectionChanged oldCollection) {
				if(_collectionChangedHandlers.TryGetValue(propertyName, out var handler)) {
					oldCollection.CollectionChanged -= handler;
				}
			} else if(existing is INotifyPropertyChanged oldNotifier) {
				if(_propertyChangedHandlers.TryGetValue(propertyName, out var handler)) {
					oldNotifier.PropertyChanged -= handler;
				}
			}
		}

		private PropertyChangedEventHandler GetPropertyChangedHandler(string propertyName)
			=> new((object sender, PropertyChangedEventArgs e)
				=> OnPropertyChanged($"{propertyName}.{e.PropertyName}")
			);

		private NotifyCollectionChangedEventHandler GetCollectionChangedHandler(string propertyName)
			=> new((object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(propertyName));

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if(_dependentProperties.TryGetValue(propertyName, out var dependencies)) {
				foreach(var dependency in dependencies) {
					OnPropertyChanged(dependency);
				}
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
