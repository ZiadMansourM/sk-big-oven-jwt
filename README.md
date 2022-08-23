# 🫡 Welcome to sk-big-oven-alpinjs Using .NET 6 🤖 
Fourth task on my internship at [SilverKey](https://www.silverkeytech.com/).

<details>
<summary>🚨 Day One</summary>


```Console
** My goal for today was to:
$ Implement the plan, I had yesterday for how to manage the states with minimum complexity.
$ Finish all the reactivity related to Categories.
```
# 🦦 Checklist of the day
- [X] Implemented reactive "List, Get, Create, Update, Delete" for Category Model.

<details>
<summary> ⚠️ Logic I used</summary>

```Console
** Quick guideline for the logic I used:
$ open var with values "home, categories, recipes", for site navigation.
$ Each model aka Category or Recipe has the following states:
    > categories: []
    > activeCategory: { id: '', name: '' }
    - use it in Get, Update, Create, Delete.
    > loadCategories()
    > createCategories()
    > deleteCategory()
    > updateCategory()
    > initActiveCategory(category)
    - Initialize activeCategory.
    > cleanActiveCategory()
    - reset activeCategory.
$ on-click at Get, Update, Create, Delete buttons.
    - [1] initActiveCategory(category)
    - [2] open model and populate it with activeCategory.
    - [3] Manipulate activeCategory data.
```

</details>

## More Details:
<details>
<summary> 🧐 Code</summary>

```html
<div 
  x-data="state()"
  x-init="loadCategories(); loadRecipes();"
  class="container d-flex flex-column min-vh-100 p-3"
>
```

```js
<script>
   function state() {
    return {
     open: 'home',
     categories: [],
     baseAddress: '@Program.config["BaseAddress"]',
     activeCategory: { id: '', name: '' },
     loadCategories() {
      fetch(`${this.baseAddress}/categories`)
       .then(res => res.json())
       .then(categories => this.categories = categories)
     },
     createCategories() {
      fetch(`${this.baseAddress}/categories`, {
       method: 'POST',
       headers: { 
        'Content-Type': 'application/json'
       },
       body: JSON.stringify(this.activeCategory.name)
      })
      .then(response => {
       if (response.ok)
        return response.json();
       throw new Error('Error ...');
      })
      .then(jsonResponse => this.categories.push(jsonResponse))
      .catch(err => console.log(err));
      this.cleanActiveCategory();
     },
     deleteCategory() {
      fetch(`${this.baseAddress}/categories/${this.activeCategory.id}`, {
       method: 'DELETE',
       headers: { 
        'Content-Type': 'application/json'
       }
      })
      .then(response => {
       if (response.ok)
        return response.json();
       throw new Error('Error ...');
      })
      .then(jsonResponse => console.log(jsonResponse))
      .catch(err => console.log(err));
      var found = this.categories.find(category => { return category.id == this.activeCategory.id });
      if(found)
       this.categories.splice(this.categories.indexOf(found), 1);
      this.cleanActiveCategory();
     },
     updateCategory() {
      fetch(`${this.baseAddress}/categories/${this.activeCategory.id}`, {
       method: 'PUT',
       headers: { 
        'Content-Type': 'application/json'
       },
       body: JSON.stringify(this.activeCategory)
      })
      .then(response => {
       if (response.ok)
        return response.json();
       throw new Error('Error ...');
      })
      .then(jsonResponse => console.log(jsonResponse))
      .catch(err => console.log(err));
      var found = this.categories.find(category => { return category.id == this.activeCategory.id });
      if(found)
       this.categories.splice(this.categories.indexOf(found), 1, this.activeCategory);
      this.cleanActiveCategory();
     },
     initActiveCategory(category) {
      this.activeCategory.id=category.id; 
      this.activeCategory.name=category.name;
     },
     cleanActiveCategory() { this.activeCategory={ id: '', name: '' } },
    }
   }
</script>
```

</details>

ToDo:
- [ ] Deploy Front && Backend.
```
- I should have done it from the start of ex4 but wanted all my attention with AlpineJS.
```
- [ ] Fix JS Coding Conventions "Spacing && Naming ...".
- [ ] Implement reactive "List, Get, Create, Update, Delete" for Recipe Model.

</details>
