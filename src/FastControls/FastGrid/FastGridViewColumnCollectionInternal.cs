﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FastGrid.FastGrid.Data;

namespace FastGrid.FastGrid
{
    internal class FastGridViewColumnCollectionInternal : FastGridViewColumnCollection, IDisposable {
        private FastGridViewDataHolder _self = null;

        private List<FastGridViewColumn> _oldColumns = new List<FastGridViewColumn>();
        // if null -> force recreation
        private List<FastGridViewColumn> _sortedColumns = null;

        internal FastGridViewDataHolder DataHolder {
            set {
                Debug.Assert(_self == null);
                _self = value;
                Subscribe();
            }
        }

        public FastGridViewColumnCollectionInternal() {
            CollectionChanged += FastGridViewColumnCollectionInternal_CollectionChanged;
        }

        private FastGridViewColumnCollectionInternal(IEnumerable<FastGridViewColumn> list) : base(list) {
            CollectionChanged += FastGridViewColumnCollectionInternal_CollectionChanged;
            _oldColumns = this.ToList();
            Subscribe();
        }

        private void FastGridViewColumnCollectionInternal_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            Unsubscribe();
            _oldColumns = this.ToList();
            Subscribe();
            _self?.OnColumnsCollectionChanged();
        }

        private void BuildSortedColumns() {
            if (_sortedColumns == null)
                _sortedColumns = this.OrderBy(c => c.DisplayIndex).ToList();
        }

        public int GetColumnIndex(FastGridViewColumn column) {
            BuildSortedColumns();
            var idx = FastGridUtil.RefIndex(_sortedColumns, column);
            return idx;
        }

        private void Unsubscribe() {
            foreach (var col in _oldColumns)
                col.PropertyChanged -= Col_PropertyChanged;
        }

        private void Col_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "DisplayIndex": 
                    _sortedColumns = null;
                    break;
            }

            _self?.OnColumnPropertyChanged(sender as FastGridViewColumn, e.PropertyName);
        }

        private void Subscribe() {
            foreach (var col in _oldColumns)
                col.PropertyChanged += Col_PropertyChanged;
        }

        // you still need to set DataHolder afterwards
        public FastGridViewColumnCollectionInternal Clone() {
            FastGridViewColumnCollectionInternal clone = new FastGridViewColumnCollectionInternal(this.Select(c => c.Clone()));
            return clone;
        }

        public void Dispose() {
            CollectionChanged -= FastGridViewColumnCollectionInternal_CollectionChanged;
            Unsubscribe();
        }

        public void EnsureExpandColumn() {
            if (Count > 0 && this[0].UniqueName != FastGridUtil.EXPANDER_COLUMN) 
                Insert(0, FastGridUtil.NewExpanderColumn());
        }
    }
}
