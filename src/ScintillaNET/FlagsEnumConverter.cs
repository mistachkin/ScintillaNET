/*
 * FlagsEnumConverter.cs --
 *
 * Copyright (c) 2017 Jacob Slusser, https://github.com/jacobslusser
 *
 * This file is part of ScintillaNET, which is distributed under the MIT
 * License; see the file "LICENSE" for full terms and a DISCLAIMER OF ALL
 * WARRANTIES.
 *
 * RCS: @(#) $Id: $
 */

using System;
using System.ComponentModel;
using System.Reflection;
using ScintillaNET;

namespace FlagsEnumTypeConverter
{
    // http://www.codeproject.com/Articles/14518/Bit-Flags-Type-Converter
    /// <summary>
    /// Implements a type converter that presents each flag of a bit-flags
    /// enumeration as a separate boolean field in the property grid.
    /// </summary>
    [ObjectId("2b4e6563-fce8-4ef1-a691-82be17bf073c")]
    internal class FlagsEnumConverter : EnumConverter
    {
        #region Public Constructors
        /// <summary>
        /// Constructs an instance of the FlagsEnumConverter class.
        /// </summary>
        /// <param name="type">
        /// The type of the enumeration.
        /// </param>
        public FlagsEnumConverter(
            Type type /* in */
            )
            : base(type)
        {
            // do nothing.
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Retrieves the property descriptors for the enumeration fields.
        /// These property descriptors will be used by the property grid to
        /// show separate enumeration fields.
        /// </summary>
        /// <param name="context">
        /// The current context.
        /// </param>
        /// <param name="value">
        /// A value of an enumeration type.
        /// </param>
        /// <param name="attributes">
        /// An array of attributes used as a filter.
        /// </param>
        /// <returns>
        /// The collection of property descriptors for the enumeration fields.
        /// </returns>
        public override PropertyDescriptorCollection GetProperties(
            ITypeDescriptorContext context, /* in */
            object value,                   /* in */
            Attribute[] attributes          /* in */
            )
        {
            if (context != null)
            {
                Type myType = value.GetType();
                string[] myNames = Enum.GetNames(myType);
                Array myValues = Enum.GetValues(myType);

                if (myNames != null)
                {
                    PropertyDescriptorCollection myCollection =
                        new PropertyDescriptorCollection(null);

                    for (int i = 0; i < myNames.Length; i++)
                    {
                        if ((int)myValues.GetValue(i) != 0 &&
                                myNames[i] != "All")
                        {
                            myCollection.Add(new EnumFieldDescriptor(
                                myType, myNames[i], context));
                        }
                    }

                    return myCollection;
                }
            }

            return base.GetProperties(context, value, attributes);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves a value indicating whether this object supports
        /// properties.
        /// </summary>
        /// <param name="context">
        /// The current context.
        /// </param>
        /// <returns>
        /// True if this object supports properties; otherwise, the value
        /// returned by the base implementation.
        /// </returns>
        public override bool GetPropertiesSupported(
            ITypeDescriptorContext context /* in */
            )
        {
            if (context != null)
            {
                return true;
            }

            return base.GetPropertiesSupported(context);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves a value indicating whether this object supports a
        /// standard set of values that can be picked from a list.
        /// </summary>
        /// <param name="context">
        /// The current context.
        /// </param>
        /// <returns>
        /// False, as this converter does not supply a standard set of values.
        /// </returns>
        public override bool GetStandardValuesSupported(
            ITypeDescriptorContext context /* in */
            )
        {
            return false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Nested Types
        /// <summary>
        /// This class represents an enumeration field in the property grid.
        /// </summary>
        [ObjectId("cf1e6761-785d-4e3e-ae2c-ce675db9106f")]
        protected class EnumFieldDescriptor : SimplePropertyDescriptor
        {
            #region Private Data
            /// <summary>
            /// Stores the context which the enumeration field descriptor was
            /// created in.
            /// </summary>
            private ITypeDescriptorContext fContext;
            #endregion

            ///////////////////////////////////////////////////////////////////

            #region Public Constructors
            /// <summary>
            /// Creates an instance of the enumeration field descriptor class.
            /// </summary>
            /// <param name="componentType">
            /// The type of the enumeration.
            /// </param>
            /// <param name="name">
            /// The name of the enumeration field.
            /// </param>
            /// <param name="context">
            /// The current context.
            /// </param>
            public EnumFieldDescriptor(
                Type componentType,            /* in */
                string name,                   /* in */
                ITypeDescriptorContext context /* in */
                )
                : base(componentType, name, typeof(bool))
            {
                fContext = context;
            }
            #endregion

            ///////////////////////////////////////////////////////////////////

            #region Public Properties
            /// <summary>
            /// Gets the collection of attributes for this enumeration field.
            /// </summary>
            public override AttributeCollection Attributes
            {
                get
                {
                    return new AttributeCollection(new Attribute[] {
                        RefreshPropertiesAttribute.Repaint });
                }
            }
            #endregion

            ///////////////////////////////////////////////////////////////////

            #region Public Methods
            /// <summary>
            /// Retrieves the value of the enumeration field.
            /// </summary>
            /// <param name="component">
            /// The instance of the enumeration type which to retrieve the
            /// field value for.
            /// </param>
            /// <returns>
            /// True if the enumeration field is included in the enumeration;
            /// otherwise, false.
            /// </returns>
            public override object GetValue(
                object component /* in */
                )
            {
                return ((int)component &
                    (int)Enum.Parse(ComponentType, Name)) != 0;
            }

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Sets the value of the enumeration field.
            /// </summary>
            /// <param name="component">
            /// The instance of the enumeration type which to set the field
            /// value to.
            /// </param>
            /// <param name="value">
            /// True if the enumeration field should be included in the
            /// enumeration; otherwise, false.
            /// </param>
            public override void SetValue(
                object component, /* in */
                object value      /* in */
                )
            {
                bool myValue = (bool)value;
                int myNewValue;

                if (myValue)
                {
                    myNewValue = ((int)component) |
                        (int)Enum.Parse(ComponentType, Name);
                }
                else
                {
                    myNewValue = ((int)component) &
                        ~(int)Enum.Parse(ComponentType, Name);
                }

                FieldInfo myField = component.GetType().GetField(
                    "value__", BindingFlags.Instance | BindingFlags.Public);
                myField.SetValue(component, myNewValue);
                fContext.PropertyDescriptor.SetValue(
                    fContext.Instance, component);
            }

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Retrieves a value indicating whether the enumeration field is
            /// set to a non-default value.
            /// </summary>
            /// <param name="component">
            /// The instance of the enumeration type to inspect.
            /// </param>
            /// <returns>
            /// True if the enumeration field differs from its default value;
            /// otherwise, false.
            /// </returns>
            public override bool ShouldSerializeValue(
                object component /* in */
                )
            {
                return (bool)GetValue(component) != GetDefaultValue();
            }

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Resets the enumeration field to its default value.
            /// </summary>
            /// <param name="component">
            /// The instance of the enumeration type whose field is reset.
            /// </param>
            public override void ResetValue(
                object component /* in */
                )
            {
                SetValue(component, GetDefaultValue());
            }

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// Retrieves a value indicating whether the enumeration field can
            /// be reset to the default value.
            /// </summary>
            /// <param name="component">
            /// The instance of the enumeration type to inspect.
            /// </param>
            /// <returns>
            /// True if the enumeration field can be reset to its default
            /// value; otherwise, false.
            /// </returns>
            public override bool CanResetValue(
                object component /* in */
                )
            {
                return ShouldSerializeValue(component);
            }
            #endregion

            ///////////////////////////////////////////////////////////////////

            #region Private Methods
            /// <summary>
            /// Retrieves the enumeration field's default value.
            /// </summary>
            /// <returns>
            /// True if the enumeration field is included in the default value
            /// of the enumeration; otherwise, false.
            /// </returns>
            private bool GetDefaultValue()
            {
                object myDefaultValue = null;
                string myPropertyName = fContext.PropertyDescriptor.Name;
                Type myComponentType =
                    fContext.PropertyDescriptor.ComponentType;

                // Get DefaultValueAttribute
                DefaultValueAttribute myDefaultValueAttribute =
                    (DefaultValueAttribute)Attribute.GetCustomAttribute(
                        myComponentType.GetProperty(myPropertyName,
                            BindingFlags.Instance | BindingFlags.Public |
                            BindingFlags.NonPublic),
                        typeof(DefaultValueAttribute));

                if (myDefaultValueAttribute != null)
                    myDefaultValue = myDefaultValueAttribute.Value;

                if (myDefaultValue != null)
                    return ((int)myDefaultValue &
                        (int)Enum.Parse(ComponentType, Name)) != 0;

                return false;
            }
            #endregion
        }
        #endregion
    }
}
