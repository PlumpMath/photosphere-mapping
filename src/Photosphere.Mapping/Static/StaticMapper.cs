﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Photosphere.Mapping.Extensions;
using Photosphere.Mapping.Static.Exceptions;

namespace Photosphere.Mapping.Static
{
    internal static class StaticMapper<TSource, TTarget>
        where TTarget : new()
    {
        private static readonly Action<TSource, TTarget> ProduceMapping;

        static StaticMapper()
        {
            ProduceMapping = MapFunctionGenerator.Generate<TSource, TTarget>();
        }

        public static TTarget Map(TSource source)
        {
            var target = new TTarget();
            ProduceMapping(source, target);
            return target;
        }

        public static void Map(TSource source, TTarget target)
        {
            ProduceMapping(source, target);
        }
    }

    internal static class StaticMapper
    {
        private static readonly RegisteredMapActions MapActions = new RegisteredMapActions();

        public static void Map(object source, object target)
        {
            CheckType(target);
            var mapAction = GetMapAction(source.GetType(), target.GetType());
            mapAction(source, target);
        }

        public static TTarget Map<TTarget>(object source)
            where TTarget : new()
        {
            var target = new TTarget();
            Map(source, target);
            return target;
        }

        private static void CheckType(object target)
        {
            if (target.GetType().IsAnonymous())
            {
                throw new MapToAnonymousTypeException();
            };
        }

        private static Action<object, object> GetMapAction(Type sourceType, Type targetType)
        {
            var typesPair = new KeyValuePair<Type, Type>(sourceType, targetType);
            Action<object, object> action;
            if (MapActions.TryGetValue(typesPair, out action))
            {
                return action;
            }
            action = MapFunctionGenerator.Generate(sourceType, targetType);
            MapActions.AddOrRewrite(typesPair, action);
            return action;
        }

        private sealed class RegisteredMapActions : ConcurrentDictionary<KeyValuePair<Type, Type>, Action<object, object>> { }
    }
}