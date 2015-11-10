// <copyright company="SIX Networks GmbH" file="ListGamesQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.DataModels.Games;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class ListGamesQuery : IRequest<SixReactiveDisposableList<GameDataModel>> {}

    [StayPublic]
    public class ListGamesQueryHandler : IRequestHandler<ListGamesQuery, SixReactiveDisposableList<GameDataModel>>
    {
        readonly IGameContext _context;
        readonly IGameMapperConfig _gameMapper;

        public ListGamesQueryHandler(IGameContext context, IGameMapperConfig gameMapper) {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(gameMapper != null);

            _context = context;
            _gameMapper = gameMapper;
        }

        public SixReactiveDisposableList<GameDataModel> Handle(ListGamesQuery query) {
            var items = _context.Games.OrderBy(x => x.MetaData.ReleasedOn);
            var mappedList = _gameMapper.Map<SixReactiveDisposableList<GameDataModel>>(items);
            SetupObservable(items, mappedList);
            return mappedList;
        }

        void SetupObservable(IEnumerable<Game> items, SixReactiveDisposableList<GameDataModel> mappedList) {
            mappedList.ChangeTrackingEnabled = true;
            var disposables = new CompositeDisposable();

            try {
                var oOut = mappedList.ItemChanged;
                disposables.Add(oOut.Where(x => x.PropertyName == "IsFavorite").Subscribe(x => {
                    var item = GetItem(items, x.Sender.Id);
                    item.IsFavorite = x.Sender.IsFavorite;
                }));

                var list = new ReactiveList<Game>(items) {ChangeTrackingEnabled = true};
                var o = list.ItemChanged;
                disposables.Add(o.Where(x => x.PropertyName == "InstalledState").Subscribe(x => {
                    var item = GetItem(mappedList, x.Sender.Id);
                    _gameMapper.Map(x.Sender.InstalledState, item);
                }));
                disposables.Add(o.Where(x => x.PropertyName == "StartupLine").Subscribe(x => {
                    var item = GetItem(mappedList, x.Sender.Id);
                    item.StartupLine = x.Sender.StartupLine;
                }));
                disposables.Add(o.Where(x => x.PropertyName == "Running").Subscribe(x => {
                    var item = GetItem(mappedList, x.Sender.Id);
                    item.Running = x.Sender.Running;
                }));

                mappedList.SetDisposables(disposables);
            } catch (Exception) {
                disposables.Dispose();
                throw;
            }
        }

        static Game GetItem(IEnumerable<Game> list, Guid id) {
            return list.First(y => y.Id == id);
        }

        static GameDataModel GetItem(IEnumerable<GameDataModel> mappedList, Guid id) {
            return mappedList.First(y => y.Id == id);
        }
    }
}