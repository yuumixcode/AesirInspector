using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// The context of a given deserialization session. This class maintains all internal and external references during deserialization.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.Utilities.ICacheNotificationReceiver" />
	public sealed class DeserializationContext : ICacheNotificationReceiver
	{
		private SerializationConfig config;

		private Dictionary<int, object> internalIdReferenceMap = new Dictionary<int, object>(128);

		private StreamingContext streamingContext;

		private IFormatterConverter formatterConverter;

		private TwoWaySerializationBinder binder;

		/// <summary>
		/// Gets or sets the context's type binder.
		/// </summary>
		/// <value>
		/// The context's serialization binder.
		/// </value>
		public TwoWaySerializationBinder Binder
		{
			get
			{
				if (binder == null)
				{
					binder = TwoWaySerializationBinder.Default;
				}
				return binder;
			}
			set
			{
				binder = value;
			}
		}

		/// <summary>
		/// Gets or sets the string reference resolver.
		/// </summary>
		/// <value>
		/// The string reference resolver.
		/// </value>
		public IExternalStringReferenceResolver StringReferenceResolver { get; set; }

		/// <summary>
		/// Gets or sets the Guid reference resolver.
		/// </summary>
		/// <value>
		/// The Guid reference resolver.
		/// </value>
		public IExternalGuidReferenceResolver GuidReferenceResolver { get; set; }

		/// <summary>
		/// Gets or sets the index reference resolver.
		/// </summary>
		/// <value>
		/// The index reference resolver.
		/// </value>
		public IExternalIndexReferenceResolver IndexReferenceResolver { get; set; }

		/// <summary>
		/// Gets the streaming context.
		/// </summary>
		/// <value>
		/// The streaming context.
		/// </value>
		public StreamingContext StreamingContext => streamingContext;

		/// <summary>
		/// Gets the formatter converter.
		/// </summary>
		/// <value>
		/// The formatter converter.
		/// </value>
		public IFormatterConverter FormatterConverter => formatterConverter;

		/// <summary>
		/// Gets or sets the serialization configuration.
		/// </summary>
		/// <value>
		/// The serialization configuration.
		/// </value>
		public SerializationConfig Config
		{
			get
			{
				if (config == null)
				{
					config = new SerializationConfig();
				}
				return config;
			}
			set
			{
				config = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.DeserializationContext" /> class.
		/// </summary>
		public DeserializationContext()
			: this(default(StreamingContext), new FormatterConverter())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.DeserializationContext" /> class.
		/// </summary>
		/// <param name="context">The streaming context to use.</param>
		public DeserializationContext(StreamingContext context)
			: this(context, new FormatterConverter())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.DeserializationContext" /> class.
		/// </summary>
		/// <param name="formatterConverter">The formatter converter to use.</param>
		public DeserializationContext(FormatterConverter formatterConverter)
			: this(default(StreamingContext), formatterConverter)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.DeserializationContext" /> class.
		/// </summary>
		/// <param name="context">The streaming context to use.</param>
		/// <param name="formatterConverter">The formatter converter to use.</param>
		/// <exception cref="T:System.ArgumentNullException">The formatterConverter parameter is null.</exception>
		public DeserializationContext(StreamingContext context, FormatterConverter formatterConverter)
		{
			if (formatterConverter == null)
			{
				throw new ArgumentNullException("formatterConverter");
			}
			streamingContext = context;
			this.formatterConverter = formatterConverter;
			Reset();
		}

		/// <summary>
		/// Registers an internal reference to a given id.
		/// </summary>
		/// <param name="id">The id to register the reference with.</param>
		/// <param name="reference">The reference to register.</param>
		public void RegisterInternalReference(int id, object reference)
		{
			internalIdReferenceMap[id] = reference;
		}

		/// <summary>
		/// Gets an internal reference from a given id, or null if the id has not been registered.
		/// </summary>
		/// <param name="id">The id of the reference to get.</param>
		/// <returns>An internal reference from a given id, or null if the id has not been registered.</returns>
		public object GetInternalReference(int id)
		{
			internalIdReferenceMap.TryGetValue(id, out var result);
			return result;
		}

		/// <summary>
		/// Gets an external object reference by index, or null if the index could not be resolved.
		/// </summary>
		/// <param name="index">The index to resolve.</param>
		/// <returns>An external object reference by the given index, or null if the index could not be resolved.</returns>
		public object GetExternalObject(int index)
		{
			if (IndexReferenceResolver == null)
			{
				Config.DebugContext.LogWarning("Tried to resolve external reference by index (" + index + "), but no index reference resolver is assigned to the deserialization context. External reference has been lost.");
				return null;
			}
			if (IndexReferenceResolver.TryResolveReference(index, out var result))
			{
				return result;
			}
			Config.DebugContext.LogWarning("Failed to resolve external reference by index (" + index + "); the index resolver could not resolve the index. Reference lost.");
			return null;
		}

		/// <summary>
		/// Gets an external object reference by guid, or null if the guid could not be resolved.
		/// </summary>
		/// <param name="guid">The guid to resolve.</param>
		/// <returns>An external object reference by the given guid, or null if the guid could not be resolved.</returns>
		public object GetExternalObject(Guid guid)
		{
			Guid guid2;
			if (GuidReferenceResolver == null)
			{
				DebugContext debugContext = Config.DebugContext;
				guid2 = guid;
				debugContext.LogWarning("Tried to resolve external reference by guid (" + guid2.ToString() + "), but no guid reference resolver is assigned to the deserialization context. External reference has been lost.");
				return null;
			}
			for (IExternalGuidReferenceResolver resolver = GuidReferenceResolver; resolver != null; resolver = resolver.NextResolver)
			{
				if (resolver.TryResolveReference(guid, out var result))
				{
					return result;
				}
			}
			DebugContext debugContext2 = Config.DebugContext;
			guid2 = guid;
			debugContext2.LogWarning("Failed to resolve external reference by guid (" + guid2.ToString() + "); no guid resolver could resolve the guid. Reference lost.");
			return null;
		}

		/// <summary>
		/// Gets an external object reference by an id string, or null if the id string could not be resolved.
		/// </summary>
		/// <param name="id">The id string to resolve.</param>
		/// <returns>An external object reference by an id string, or null if the id string could not be resolved.</returns>
		public object GetExternalObject(string id)
		{
			if (StringReferenceResolver == null)
			{
				Config.DebugContext.LogWarning("Tried to resolve external reference by string (" + id + "), but no string reference resolver is assigned to the deserialization context. External reference has been lost.");
				return null;
			}
			for (IExternalStringReferenceResolver resolver = StringReferenceResolver; resolver != null; resolver = resolver.NextResolver)
			{
				if (resolver.TryResolveReference(id, out var result))
				{
					return result;
				}
			}
			Config.DebugContext.LogWarning("Failed to resolve external reference by string (" + id + "); no string resolver could resolve the string. Reference lost.");
			return null;
		}

		/// <summary>
		/// Resets the deserialization context completely to baseline status, as if its constructor has just been called.
		/// This allows complete reuse of a deserialization context, with all of its internal reference buffers.
		/// </summary>
		public void Reset()
		{
			if (config != null)
			{
				config.ResetToDefault();
			}
			internalIdReferenceMap.Clear();
			IndexReferenceResolver = null;
			GuidReferenceResolver = null;
			StringReferenceResolver = null;
			binder = null;
		}

		void ICacheNotificationReceiver.OnFreed()
		{
			Reset();
		}

		void ICacheNotificationReceiver.OnClaimed()
		{
		}
	}
}
