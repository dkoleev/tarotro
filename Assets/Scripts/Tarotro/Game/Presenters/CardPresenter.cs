using System;
using Tarotro.Game.Logic;
using Tarotro.Game.View;

namespace Tarotro.Game.Presenters {
    public class CardPresenter : IDisposable {
        private readonly CardModel _model;
        private readonly ICardView _view;

        public CardPresenter(CardModel model, ICardView view) {
            _model = model;
            _view = view;
            _view.SetCardInfo(_model.Name, _model.Damage);
        }

        public void SetHighlighted(bool highlighted) => _view.SetHighlighted(highlighted);
        public void SetSelected(bool selected) => _view.SetSelected(selected);

        public void Dispose() { }
    }
}
