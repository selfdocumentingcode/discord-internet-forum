using DifBot.Common.Models;
using DifBot.Common.Models.Forums;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace DifBot.Data;

public class BotDbContext : DbContext
{
    private const string JsonbColumnType = "jsonb";
    private const string ForumSchemaName = "forum";

    public BotDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ConfigureMessageRefEntities(builder);

        ConfigureForumEntities(builder);

        builder.Entity<BotSettting>()
            .HasIndex(x => x.Key)
            .IsUnique();
    }

    private static void ConfigureMessageRefEntities(ModelBuilder builder)
    {
        builder.Entity<MessageRef>()
            .HasKey(x => x.MessageId);

        builder.Entity<MessageRef>()
            .Property(x => x.MessageId)
            .ValueGeneratedNever();
    }

    private static void ConfigureForumEntities(ModelBuilder builder)
    {
        builder.ConfigureEnumLookupTable<ConsentType>(nameof(ConsentTypes), ForumSchemaName);

        builder.Entity<ForumChannel>()
            .ToTable(nameof(ForumChannels), ForumSchemaName)
            .HasKey(x => x.DiscordChannelId);

        builder.Entity<ForumThread>(e =>
        {
            e.ToTable(nameof(ForumThreads), ForumSchemaName)
                .HasKey(x => x.DiscordThreadId);

            e.HasOne(x => x.ForumChannel)
                .WithMany()
                .HasForeignKey(x => x.ForumChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Members)
                .WithOne()
                .HasForeignKey(x => x.ForumThreadId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ForumOriginalPost>(e =>
        {
            e.ToTable(nameof(ForumOriginalPosts), ForumSchemaName);

            e.HasOne<ForumThread>()
                .WithOne(x => x.OriginalPost)
                .HasForeignKey<ForumOriginalPost>(x => x.OriginalPostForumThreadId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ForumReplyPost>(e =>
        {
            e.ToTable(nameof(ForumReplyPosts), ForumSchemaName);

            e.HasOne(x => x.ReplyTo)
                .WithMany()
                .HasForeignKey(x => x.ReplyToId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne<ForumThread>()
                .WithMany(x => x.ReplyPosts)
                .HasForeignKey(x => x.ReplyPostForumThreadId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ForumPost>(e =>
        {
            e.ToTable(nameof(ForumPosts), ForumSchemaName)
                .HasKey(x => x.DiscordMessageId);

            e.HasOne<ForumThread>()
                .WithMany()
                .HasForeignKey(x => x.ForumThreadId)
                .OnDelete(DeleteBehavior.Restrict);

            const string authorDiscordUserIdColumn = $"{nameof(ForumPost.Author)}{nameof(ThreadMember.DiscordUserId)}";
            const string authorForumThreadIdColumn = $"{nameof(ForumPost.Author)}{nameof(ThreadMember.ForumThreadId)}";

            e.Property<ulong>(authorDiscordUserIdColumn);
            e.Property<ulong>(authorForumThreadIdColumn);

            e.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(authorDiscordUserIdColumn, authorForumThreadIdColumn);
        });

        builder.Entity<ThreadMember>(e =>
        {
            e.ToTable(nameof(ThreadMembers), ForumSchemaName)
                .HasKey(x => new { x.DiscordUserId, x.ForumThreadId });

            e.HasIndex(x => x.ForumThreadId)
                .HasFilter($"\"{nameof(ThreadMembers)}\".\"{nameof(ThreadMember.IsThreadAuthor)}\" is true")
                .IsUnique();
        });

        builder.Entity<PostReaction>(e =>
        {
            e.ToTable(nameof(PostReactions), ForumSchemaName)
                .HasDiscriminator<string>(x => x.PostReactionType)
                .HasValue<DiscordEmojiReaction>(nameof(DiscordEmojiReaction))
                .HasValue<UnicodeEmojiReaction>(nameof(UnicodeEmojiReaction));

            e.Property(x => x.Id)
                .HasValueGenerator<SnowflakeGenerator>();

            e.HasOne<ForumPost>()
                .WithMany(x => x.Reactions)
                .HasForeignKey(x => x.ForumPostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DiscordEmojiReaction>(e =>
        {
            e.HasOne(x => x.Emoji)
                .WithMany()
                .HasForeignKey(x => new { x.DiscordEmojiId, x.DiscordEmojiName })
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.DiscordEmojiId, x.DiscordEmojiName, x.ForumPostId })
                .IsUnique();
        });

        builder.Entity<UnicodeEmojiReaction>(e =>
        {
            e.HasOne(x => x.Emoji)
                .WithMany()
                .HasForeignKey(x => x.EmojiCodePoint)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.EmojiCodePoint, x.ForumPostId })
                .IsUnique();
        });

        builder.Entity<DiscordEmoji>()
            .ToTable(nameof(DiscordEmojis), ForumSchemaName)
            .HasKey(x => new { x.DiscordEmojiId, x.DiscordEmojiName });

        builder.Entity<UnicodeEmoji>()
            .ToTable(nameof(UnicodeEmojis), ForumSchemaName)
            .HasKey(x => x.CodePoint);

        builder.Entity<PostAttachment>(e =>
        {
            e.ToTable(nameof(PostAttachments), ForumSchemaName)
                .HasKey(x => x.DiscordAttachmentId);

            e.HasOne<ForumPost>()
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.ForumPostId);
        });

        builder.Entity<PublishedThread>(e =>
        {
            e.ToTable(nameof(PublishedThreads), ForumSchemaName)
                .HasKey(x => x.DiscordThreadId);

            e.Property(x => x.ForumThreadJson)
                .HasColumnType(JsonbColumnType);
        });

        builder.Entity<PublishedForum>(e =>
        {
            e.ToTable(nameof(PublishedForums), ForumSchemaName)
                .HasKey(x => x.DiscordForumId);

            e.Property(x => x.ForumChannelJson)
                .HasColumnType(JsonbColumnType);
        });
    }

#pragma warning disable SA1201 // Elements should appear in the correct order
    public DbSet<BotSettting> GuildSettings { get; set; }

    public DbSet<MessageRef> MessageRefs { get; set; }

    public DbSet<UnicodeEmoji> UnicodeEmojies { get; set; }

    public DbSet<DiscordEmoji> DiscordEmojies { get; set; }

    public DbSet<ForumChannel> ForumChannels { get; set; }

    public DbSet<ForumThread> ForumThreads { get; set; }

    public DbSet<ThreadMember> ThreadMembers { get; set; }

    public DbSet<ForumPost> ForumPosts { get; set; }

    public DbSet<ForumReplyPost> ForumReplyPosts { get; set; }

    public DbSet<ForumOriginalPost> ForumOriginalPosts { get; set; }

    public DbSet<PostReaction> PostReactions { get; set; }

    public DbSet<DiscordEmojiReaction> DiscordEmojiReactions { get; set; }

    public DbSet<UnicodeEmojiReaction> UnicodeEmojiReactions { get; set; }

    public DbSet<PostAttachment> PostAttachments { get; set; }

    public DbSet<DiscordEmoji> DiscordEmojis { get; set; }

    public DbSet<UnicodeEmoji> UnicodeEmojis { get; set; }

    public DbSet<EnumLookup<ConsentType>> ConsentTypes { get; set; }

    public DbSet<PublishedThread> PublishedThreads { get; set; }

    public DbSet<PublishedForum> PublishedForums { get; set; }
}
