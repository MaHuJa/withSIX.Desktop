// <copyright company="SIX Networks GmbH" file="Post.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Connect
{
    public class Post : ConnectModelBase
    {
        string _content;
        string _slug;
        string _summary;
        string _title;
        public Post(Guid id) : base(id) {}
        public string Summary
        {
            get { return _summary; }
            set { SetProperty(ref _summary, value); }
        }
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }
        public Account Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Slug
        {
            get { return _slug; }
            set { SetProperty(ref _slug, value); }
        }

        public void Visit() {
            BrowserHelper.TryOpenUrlIntegrated(GetMyUrl());
        }

        Uri GetMyUrl() {
            return Tools.Transfer.JoinUri(CommonUrls.MainUrl, "blog", Slug);
        }
    }
}