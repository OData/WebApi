//-----------------------------------------------------------------------------
// <copyright file="Events.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Nop.Core.Domain.Messages
{
    public class EmailSubscribedEvent
    {
        private readonly string _email;

        public EmailSubscribedEvent(string email)
        {
            _email = email;
        }

        public string Email
        {
            get { return _email; }
        }

        public bool Equals(EmailSubscribedEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._email, _email);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(EmailSubscribedEvent)) return false;
            return Equals((EmailSubscribedEvent)obj);
        }

        public override int GetHashCode()
        {
            return (_email != null ? _email.GetHashCode() : 0);
        }
    }

    public class EmailUnsubscribedEvent
    {
        private readonly string _email;

        public EmailUnsubscribedEvent(string email)
        {
            _email = email;
        }

        public string Email
        {
            get { return _email; }
        }

        public bool Equals(EmailUnsubscribedEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._email, _email);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(EmailUnsubscribedEvent)) return false;
            return Equals((EmailUnsubscribedEvent)obj);
        }

        public override int GetHashCode()
        {
            return (_email != null ? _email.GetHashCode() : 0);
        }
    }

    /// <summary>
    /// A container for tokens that are added.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityTokensAddedEvent<TEntity, TItem> where TEntity : BaseEntity
    {
        private readonly TEntity _entity;
        private readonly IList<TItem> _tokens;

        public EntityTokensAddedEvent(TEntity entity, IList<TItem> tokens)
        {
            _entity = entity;
            _tokens = tokens;
        }

        public TEntity Entity
        {
            get
            {
                return _entity;
            }
        }

        public IList<TItem> Tokens
        {
            get
            {
                return _tokens;
            }
        }
    }

    /// <summary>
    /// A container for tokens that are added.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class MessageTokensAddedEvent<TItem>
    {
        private readonly MessageTemplate _message;
        private readonly IList<TItem> _tokens;

        public MessageTokensAddedEvent(MessageTemplate message, IList<TItem> tokens)
        {
            _message = message;
            _tokens = tokens;
        }

        public MessageTemplate Message
        {
            get
            {
                return _message;
            }
        }

        public IList<TItem> Tokens
        {
            get
            {
                return _tokens;
            }
        }
    }
}
