using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Arch_TL.DAL.Common;

public static class ExpressionHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Expression ExpandLambdaExpression(LambdaExpression lambda)
    {
        var expression = lambda.Body;
        if (expression.NodeType == ExpressionType.Convert)
        {
            expression = ((UnaryExpression)expression).Operand;
        }
        return expression;
    }

    public static PropertyInfo GetPropertyInfo([NotNull] this LambdaExpression lambda)
    {
        if (lambda == null) throw new ArgumentNullException(nameof(lambda));

        var expression = ExpandLambdaExpression(lambda);
        var memberExpression = (MemberExpression)expression;

        return (PropertyInfo)memberExpression.Member;
    }
    #region Delegates Cache

    private static readonly ConcurrentDictionary<Tuple<Type, MethodInfo>, Delegate> _delegates
        = new ConcurrentDictionary<Tuple<Type, MethodInfo>, Delegate>();

    private static readonly ConcurrentDictionary<Type, Func<object>> _weakDefaultConstructors
        = new ConcurrentDictionary<Type, Func<object>>();

    private static readonly ConcurrentDictionary<Type, Delegate> _strongDefaultConstructors
        = new ConcurrentDictionary<Type, Delegate>();

    private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object>> _weakPropertyInitializationGetters
        = new ConcurrentDictionary<PropertyInfo, Func<object, object>>();

    private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object>> _weakPropertyGetters
        = new ConcurrentDictionary<PropertyInfo, Func<object, object>>();

    private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object>> _weakPropertySetters
        = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();

    private static readonly ConcurrentDictionary<PropertyInfo, Delegate> _strongPropertyGetters
        = new ConcurrentDictionary<PropertyInfo, Delegate>();

    private static readonly ConcurrentDictionary<PropertyInfo, Delegate> _strongPropertySetters
        = new ConcurrentDictionary<PropertyInfo, Delegate>();

    private static readonly ConcurrentDictionary<FieldInfo, Func<object, object>> _weakFieldGetters
        = new ConcurrentDictionary<FieldInfo, Func<object, object>>();

    private static readonly ConcurrentDictionary<FieldInfo, Action<object, object>> _weakFieldSetters
        = new ConcurrentDictionary<FieldInfo, Action<object, object>>();

    private static readonly ConcurrentDictionary<FieldInfo, Delegate> _strongFieldGetters
        = new ConcurrentDictionary<FieldInfo, Delegate>();

    private static readonly ConcurrentDictionary<FieldInfo, Delegate> _strongFieldSetters
        = new ConcurrentDictionary<FieldInfo, Delegate>();

    #endregion

    #region Helper Methods

    private const BindingFlags HelperMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

    private static readonly MethodInfo _defaultConstructorHelper = typeof(ExpressionHelper).GetMethod(nameof(DefaultConstructorHelper), HelperMethodBindingFlags);

    private static readonly MethodInfo _initializationGetterHelper = typeof(ExpressionHelper).GetMethod(nameof(WeakInitializationGetterHelper), HelperMethodBindingFlags);

    private static readonly MethodInfo _weakGetterHelper = typeof(ExpressionHelper).GetMethod(nameof(WeakGetterHelper), HelperMethodBindingFlags);

    private static readonly MethodInfo _weakSetterHelper = typeof(ExpressionHelper).GetMethod(nameof(WeakSetterHelper), HelperMethodBindingFlags);

    private static readonly MethodInfo _strongGetterHelper = typeof(ExpressionHelper).GetMethod(nameof(StrongGetterHelper), HelperMethodBindingFlags);

    private static readonly MethodInfo _strongSetterHelper = typeof(ExpressionHelper).GetMethod(nameof(StrongSetterHelper), HelperMethodBindingFlags);

    #endregion

    public static Func<object, object> GetInitializationGetter(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

        return _weakPropertyInitializationGetters.GetOrAdd(propertyInfo, info => CreateWeakInitializationGetter(info));
    }

    public static bool HasDefaultConstructor(this Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        if (type.IsAbstract) return false;

        return type.GetConstructor(Type.EmptyTypes) != null;
    }


    public static Func<object> GetDefaultConstructor(this Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        return _weakDefaultConstructors.GetOrAdd(type, type1 => CreateWeakDefaultConstructor(type1));
    }

    public static Func<TInstance> GetDefaultConstructor<TInstance>()
    {
        return (Func<TInstance>)_strongDefaultConstructors.GetOrAdd(typeof(TInstance), CreateStrongDefaultConstructor<TInstance>);
    }

    public static Func<object, object> GetGetter(this FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

        return _weakFieldGetters.GetOrAdd(fieldInfo, info => CreateWeakGetter(info));
    }

    public static Action<object, object> GetSetter(this FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

        return _weakFieldSetters.GetOrAdd(fieldInfo, info => CreateWeakSetter(info));
    }

    public static Func<object, object> GetGetter(this MemberInfo memberInfo)
    {
        if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));

        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field: return ((FieldInfo)memberInfo).GetGetter();
            case MemberTypes.Property: return ((PropertyInfo)memberInfo).GetGetter();
            default: throw new InvalidOperationException($"'{memberInfo.MemberType}' is not supported.");
        }
    }

    public static Action<object, object> GetSetter(this MemberInfo memberInfo)
    {
        if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));

        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field: return ((FieldInfo)memberInfo).GetSetter();
            case MemberTypes.Property: return ((PropertyInfo)memberInfo).GetSetter();
            default: throw new InvalidOperationException($"'{memberInfo.MemberType}' is not supported.");
        }
    }

    public static Func<object, object> GetGetter(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

        return _weakPropertyGetters.GetOrAdd(propertyInfo,
            info => CreateWeakGetter(info));
    }

    public static Action<object, object> GetSetter(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

        return _weakPropertySetters.GetOrAdd(propertyInfo,
            info => CreateWeakSetter(info));
    }

    public static Func<object, TMember> GetGetter<TMember>(this MemberInfo memberInfo)
    {
        if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));

        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field: return ((FieldInfo)memberInfo).GetGetter<TMember>();
            case MemberTypes.Property: return ((PropertyInfo)memberInfo).GetGetter<TMember>();
            default: throw new InvalidOperationException($"'{memberInfo.MemberType}' is not supported.");
        }
    }

    public static Action<object, TMember> GetSetter<TMember>(this MemberInfo memberInfo)
    {
        if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));

        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field: return ((FieldInfo)memberInfo).GetSetter<TMember>();
            case MemberTypes.Property: return ((PropertyInfo)memberInfo).GetSetter<TMember>();
            default: throw new InvalidOperationException($"'{memberInfo.MemberType}' is not supported.");
        }
    }

    public static Func<object, TField> GetGetter<TField>(this FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

        return (Func<object, TField>)fieldInfo.GetGetterCore();
    }
    private static Delegate GetGetterCore(this FieldInfo fieldInfo)
    {
        return _strongFieldGetters.GetOrAdd(fieldInfo,
            info => CreateStrongGetter(info));
    }

    public static Action<object, TField> GetSetter<TField>(this FieldInfo fieldInfo)
    {
        if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

        return (Action<object, TField>)fieldInfo.GetSetterCore();
    }
    private static Delegate GetSetterCore(this FieldInfo fieldInfo)
    {
        return _strongFieldSetters.GetOrAdd(fieldInfo,
            info => CreateStrongSetter(info));
    }

    public static Func<object, TProperty> GetGetter<TProperty>(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

        return (Func<object, TProperty>)propertyInfo.GetGetterCore();
    }
    private static Delegate GetGetterCore(this PropertyInfo propertyInfo)
    {
        return _strongPropertyGetters.GetOrAdd(propertyInfo,
            info => CreateStrongGetter(info));
    }

    public static Action<object, TProperty> GetSetter<TProperty>(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

        return (Action<object, TProperty>)propertyInfo.GetSetterCore();
    }
    private static Delegate GetSetterCore(this PropertyInfo propertyInfo)
    {
        return _strongPropertySetters.GetOrAdd(propertyInfo,
            info => CreateStrongSetter(info));
    }

    #region Initialization

    private static TDelegate CreateDelegate<TDelegate>(this MethodInfo methodInfo)
        where TDelegate : Delegate
    {
        return (TDelegate)methodInfo.CreateDelegateCore(typeof(TDelegate));
    }

    private static Delegate CreateDelegateCore(this MethodInfo methodInfo, Type delegateType)
    {
        var key = Tuple.Create(delegateType, methodInfo);
        return _delegates.GetOrAdd(key, k => k.Item2.CreateDelegate(k.Item1));
    }

    private static Func<object> CreateWeakDefaultConstructor(Type type)
    {
        if (type.IsInterface) return null;
        if (type.IsAbstract) return null;
        var defaultConstructorInfo = type.GetConstructor(Type.EmptyTypes);
        if (defaultConstructorInfo == null) return null;

        var helper = _defaultConstructorHelper.MakeGenericMethod(type);

        return (Func<object>)helper.Invoke(null, Array.Empty<object>());
    }

    private static Func<TInstance> CreateStrongDefaultConstructor<TInstance>(Type type)
    {
        if (type.IsAbstract) return null;
        var defaultConstructorInfo = type.GetConstructor(Type.EmptyTypes);
        if (defaultConstructorInfo == null) return null;

        var defaultConstructorMethodInfo = GenerateDefaultConstructor(defaultConstructorInfo);

        return defaultConstructorMethodInfo.CreateDelegate<Func<TInstance>>();
    }

    private static Func<object, object> CreateWeakInitializationGetter(PropertyInfo propertyInfo)
    {
        var getMethodInfo = propertyInfo.GetGetMethod();
        if (getMethodInfo == null) return null;
        var setMethodInfo = propertyInfo.GetSetMethod();
        if (setMethodInfo == null) return null;
        var defaultConstructorInfo = propertyInfo.PropertyType.GetConstructor(Type.EmptyTypes);
        if (defaultConstructorInfo == null) return null;

        var helper = _initializationGetterHelper.MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);

        return (Func<object, object>)helper.Invoke(null, new object[] { getMethodInfo, setMethodInfo });
    }

    private static Func<object, object> CreateWeakGetter(PropertyInfo propertyInfo)
    {
        var getMethodInfo = propertyInfo.GetGetMethod();
        if (getMethodInfo == null) return null;

        var helper = _weakGetterHelper.MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);

        return (Func<object, object>)helper.Invoke(null, new object[] { getMethodInfo });
    }

    private static Action<object, object> CreateWeakSetter(PropertyInfo propertyInfo)
    {
        var setMethodInfo = propertyInfo.GetSetMethod();
        if (setMethodInfo == null) return null;

        var helper = _weakSetterHelper.MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);

        return (Action<object, object>)helper.Invoke(null, new object[] { setMethodInfo });
    }

    private static Func<object, object> CreateWeakGetter(FieldInfo fieldInfo)
    {
        if (fieldInfo.IsStatic) return null;

        var helper = _weakGetterHelper.MakeGenericMethod(fieldInfo.ReflectedType, fieldInfo.FieldType);

        var getMethodInfo = fieldInfo.GenerateFieldGetter();

        return (Func<object, object>)helper.Invoke(null, new object[] { getMethodInfo });
    }

    private static Action<object, object> CreateWeakSetter(FieldInfo fieldInfo)
    {
        if (fieldInfo.IsStatic) return null;

        var helper = _weakSetterHelper.MakeGenericMethod(fieldInfo.ReflectedType, fieldInfo.FieldType);

        var setMethodInfo = GenerateFieldSetter(fieldInfo);

        return (Action<object, object>)helper.Invoke(null, new object[] { setMethodInfo });
    }

    private static Delegate CreateStrongGetter(PropertyInfo propertyInfo)
    {
        var getMethodInfo = propertyInfo.GetGetMethod();
        if (getMethodInfo == null) return null;

        var helper = _strongGetterHelper.MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);

        return (Delegate)helper.Invoke(null, new object[] { getMethodInfo });
    }

    private static Delegate CreateStrongSetter(PropertyInfo propertyInfo)
    {
        var setMethodInfo = propertyInfo.GetSetMethod();
        if (setMethodInfo == null) return null;

        var helper = _strongSetterHelper.MakeGenericMethod(propertyInfo.ReflectedType, propertyInfo.PropertyType);

        return (Delegate)helper.Invoke(null, new object[] { setMethodInfo });
    }

    private static Delegate CreateStrongGetter(FieldInfo fieldInfo)
    {
        if (fieldInfo.IsStatic) return null;

        var helper = _strongGetterHelper.MakeGenericMethod(fieldInfo.ReflectedType, fieldInfo.FieldType);

        var getMethodInfo = fieldInfo.GenerateFieldGetter();

        return (Delegate)helper.Invoke(null, new object[] { getMethodInfo });
    }

    private static Delegate CreateStrongSetter(FieldInfo fieldInfo)
    {
        if (fieldInfo.IsStatic) return null;

        var helper = _strongSetterHelper.MakeGenericMethod(fieldInfo.ReflectedType, fieldInfo.FieldType);

        var setMethodInfo = GenerateFieldSetter(fieldInfo);

        return (Delegate)helper.Invoke(null, new object[] { setMethodInfo });
    }

    #endregion

    #region Code Generation

    private static DynamicMethod GenerateFieldGetter(this FieldInfo fieldInfo)
    {
        string methodName = fieldInfo.ReflectedType.FullName + ".x_get_" + fieldInfo.Name;

        var parameterTypes = new[] { fieldInfo.ReflectedType };
        var getMethodInfo = new DynamicMethod(methodName, fieldInfo.FieldType, parameterTypes, true);

        var gen = getMethodInfo.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldfld, fieldInfo);
        gen.Emit(OpCodes.Ret);

        return getMethodInfo;
    }

    private static DynamicMethod GenerateDefaultConstructor(ConstructorInfo constructorInfo)
    {
        string methodName = constructorInfo.ReflectedType.FullName + ".ctor_x";

        var constructorMethodInfo = new DynamicMethod(methodName, constructorInfo.DeclaringType, Type.EmptyTypes);

        var gen = constructorMethodInfo.GetILGenerator();
        //gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Newobj, constructorInfo);
        gen.Emit(OpCodes.Ret);

        return constructorMethodInfo;
    }

    private static DynamicMethod GenerateFieldSetter(FieldInfo fieldInfo)
    {
        string methodName = fieldInfo.ReflectedType.FullName + ".x_set_" + fieldInfo.Name;

        var parameterTypes = new[] { fieldInfo.ReflectedType, fieldInfo.FieldType };
        var setMethodInfo = new DynamicMethod(methodName, null, parameterTypes, true);

        var gen = setMethodInfo.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Stfld, fieldInfo);
        gen.Emit(OpCodes.Ret);

        return setMethodInfo;
    }

    #endregion

    #region Reflection Call

    private static Func<object> DefaultConstructorHelper<TInstance>()
    {
        var defaultConstructor = GetDefaultConstructor<TInstance>();

        return () => defaultConstructor();
    }

    private static Func<object, object> WeakInitializationGetterHelper<TInstance, TProperty>(MethodInfo getMethodInfo, MethodInfo setMethodInfo)
    {
        var strongDefaultConstructor = GetDefaultConstructor<TProperty>();
        var strongGetter = getMethodInfo.CreateDelegate<Func<TInstance, TProperty>>();
        var strongSetter = setMethodInfo.CreateDelegate<Action<TInstance, TProperty>>();

        return instance =>
        {
            var strongInstance = (TInstance)instance;
            var strongValue = strongGetter(strongInstance);
            if (strongValue == null)
            {
                strongValue = strongDefaultConstructor();
                strongSetter(strongInstance, strongValue);
            }

            return strongValue;
        };
    }

    private static Func<object, object> WeakGetterHelper<TInstance, TProperty>(MethodInfo getMethodInfo)
    {
        var strongGetter = getMethodInfo.CreateDelegate<Func<TInstance, TProperty>>();

        return instance => strongGetter((TInstance)instance);
    }

    private static Action<object, object> WeakSetterHelper<TInstance, TProperty>(MethodInfo setMethodInfo)
    {
        var strongSetter = setMethodInfo.CreateDelegate<Action<TInstance, TProperty>>();

        return (instance, value) => strongSetter((TInstance)instance, (TProperty)value);
    }

    private static Func<object, TProperty> StrongGetterHelper<TInstance, TProperty>(MethodInfo getMethodInfo)
    {
        var strongGetter = getMethodInfo.CreateDelegate<Func<TInstance, TProperty>>();

        return instance => strongGetter((TInstance)instance);
    }

    private static Action<object, TProperty> StrongSetterHelper<TInstance, TProperty>(MethodInfo setMethodInfo)
    {
        var strongSetter = setMethodInfo.CreateDelegate<Action<TInstance, TProperty>>();

        return (instance, value) => strongSetter((TInstance)instance, value);
    }

    #endregion
}

