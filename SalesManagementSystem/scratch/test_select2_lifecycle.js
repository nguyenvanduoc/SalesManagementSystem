// Run: node scratch/test_select2_lifecycle.js
// Regression checks for shared SPA initialization (no database/browser required).
const fs = require('fs');
const vm = require('vm');
const assert = require('assert');
const path = require('path');
const root = path.join(__dirname, '..');
const layout = fs.readFileSync(path.join(root, 'Views/Shared/_Layout.cshtml'), 'utf8');
const source = fs.readFileSync(path.join(root, 'Scripts/tab-manager.js'), 'utf8');
const cache = new Map();
let events = 0;
const document = {};
class Collection {
    constructor(elements) { this.elements = elements; this.length = elements.length; elements.forEach((e, i) => this[i] = e); }
    each(fn) { this.elements.forEach((e, i) => fn.call(e, i, e)); return this; }
    attr(key, value) { if (value === undefined) return this[0]?.attrs[key]; return this.each(function () { this.attrs[key] = value; }); }
    data(key) { return this[0]?.data[key]; }
    is(type) { return type === 'select'; }
    hasClass(name) { return this[0]?.classes.includes(name); }
    prop(key) { return this[0]?.[key]; }
    find(selector) {
        if (selector.includes('select.form-select')) return $(this[0].selects);
        if (selector === 'option[value=""]') return $(this[0].options.filter(o => o.value === ''));
        if (selector === 'option') return $(this[0].options);
        throw new Error('Unexpected selector: ' + selector);
    }
    first() { return $(this.elements.slice(0, 1)); }
    text() { return this[0]?.text || ''; }
    val() { return this[0]?.value; }
    closest() { return $(this[0]?.modal ? [this[0].modal] : []); }
    trigger() { events++; return this; }
}
const panes = {};
function $(value) {
    if (typeof value === 'string') return $([panes[value.substring(1)]]);
    return new Collection(Array.isArray(value) ? value : [value]);
}
$.fn = Collection.prototype;
$.extend = Object.assign;
$.fn.select2 = function (options) {
    if (typeof options === 'string') {
        if (options === 'destroy') this.each(function () { cache.delete(this.attrs['data-select2-id']); delete this.attrs['data-select2-id']; delete this.data.select2; });
        return this;
    }
    return this.each(function () {
        const key = this.attrs['data-select2-id'] || this.attrs.id;
        this.data.select2 = { options, element: this };
        cache.set(key, this.data.select2);
    });
};
$.fn.select2.defaults = { set() {} };
$.fn.select2.amd = { marker: true };
const context = vm.createContext({ $, document, Date, bootstrap: { Tooltip: { getOrCreateInstance() {} } } });
const wrapperStart = layout.indexOf('var originalSelect2 =');
const wrapperEnd = layout.indexOf('$.fn.select2.defaults.set', wrapperStart);
vm.runInContext(layout.slice(wrapperStart, wrapperEnd), context);
const initStart = source.indexOf('    function reInitPlugins(');
const initEnd = source.indexOf('    // ─── reloadActiveTabGrid', initStart);
vm.runInContext(source.slice(initStart, initEnd), context);
function select(id, modal) { return { attrs: { id }, data: {}, classes: [], options: [{ value: '', text: '-- Tất cả --' }, { value: '1', text: 'Customer A' }], value: '1', modal }; }
function pane(id, selects) { panes[id] = { selects, querySelectorAll() { return []; } }; }
const a = select('idKhachHang');
const b = select('idKhachHang');
const ajax = { url: '/customers', processResults: x => x };
$(a).select2({ ajax });
$(b).select2({ ajax });
assert.notStrictEqual(a.attrs['data-select2-id'], b.attrs['data-select2-id'], 'Same DOM id must have separate cache keys');
assert.strictEqual($.fn.select2.amd.marker, true, 'Preserve Select2 plugin API');
pane('tab-a', [a]);
const instance = a.data.select2;
for (let i = 0; i < 10; i++) context.reInitPlugins('tab-a', false);
assert.strictEqual(a.data.select2, instance, 'Switching tabs must preserve the instance');
assert.strictEqual(a.data.select2.options.ajax, ajax, 'Preserve AJAX transport and result mapping');
assert.strictEqual(a.attrs.id, 'idKhachHang', 'Preserve selectors/validation DOM id');
assert.strictEqual(a.value, '1', 'Preserve selection');
assert.strictEqual(events, 0, 'Do not replay content-loaded on tab switching');
$(b).select2({ ajax });
assert.notStrictEqual(a.attrs['data-select2-id'], b.attrs['data-select2-id'], 'Explicit View reinitialization must also keep separate cache keys');
$(b).select2('destroy');
assert.strictEqual(cache.get(a.attrs['data-select2-id']), instance, 'Destroying another tab must not erase this cache');
const modal = {};
const c = select('statusNewStatus', modal);
pane('tab-c', [c]);
context.reInitPlugins('tab-c');
assert.strictEqual(c.data.select2.options.dropdownParent[0], modal, 'Modal dropdown stays inside its modal');
assert.strictEqual(events, 1, 'Notify actual content initialization');
const newInstance = c.data.select2;
context.reInitPlugins('tab-c', false);
assert.strictEqual(c.data.select2, newInstance, 'Shared initialization is idempotent');
console.log('PASS: cache isolation, AJAX preservation, stable IDs/selection, tab events, modal parent, idempotence');
