﻿using Discord;
using System;

namespace FruitBot
{
    public static class EmbedUtility
    {
        public readonly static Color Primary = Color.Blue;
        public readonly static Color Secondary = Color.LightGrey;

        public static Embed FromPrimary(string title, string description, Action<EmbedBuilder> configure = null)
        {
            var builder = new EmbedBuilder
            {
                Title = title,
                Description = description,
                Color = Primary
            };

            if (configure is not null)
                configure(builder);

            return builder.Build();
        }

        public static Embed FromSecondary(string title, string description, Action<EmbedBuilder> configure = null)
        {
            var builder = new EmbedBuilder
            {
                Title = title,
                Description = description,
                Color = Secondary
            };

            if (configure is not null)
                configure(builder);

            return builder.Build();
        }

        public static Embed FromSuccess(string title, string description, bool addToTitle = true) =>
                new EmbedBuilder
                {
                    Title = addToTitle ? ":white_check_mark: " : string.Empty + title,
                    Description = addToTitle ? string.Empty : ":white_check_mark: " + description,
                    Color = Primary
                }.Build();

        public static Embed FromError(string title, string description, bool addToTitle = true) =>
            new EmbedBuilder
            {
                Title = addToTitle ? ":bangbang: " : string.Empty + title,
                Description = addToTitle ? string.Empty : ":bangbang: " + description,
                Color = Primary
            }.Build();

        public static Embed FromWarning(string title, string description, bool addToTitle = true) =>
            new EmbedBuilder
            {
                Title = addToTitle ? ":warning: " : string.Empty + title,
                Description = addToTitle ? string.Empty : ":warning: " + description,
                Color = Primary
            }.Build();
    }
}
