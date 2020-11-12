function Task(name) {
    this.name = ko.observable(name);
}
var viewModel = {
    Items: ko.observableArray([
      new Task("Content knowledge as needed for lesson planning" ),
      new Task("Planning standards-aligned lessons with clear purpose" ),
      new Task("Planning instruction appropriate for students with varied skill and ability levels" ),
      new Task("Creating supportive and challenging classroom environment" ),
      new Task("Developing efficient classroom routines for non-instructional tasks e.g., distributing materials, managing transitions" ),
      new Task("Establishing and enforcing standards of conduct in small and large group situation" ),
      new Task("Creating intellectually engaging learning tasks" ),
      new Task("Creating and using summative assessments" ),
      new Task("Using formative assessment data to make day-to-day decisions about instruction" ),
      new Task("Collaborating with colleagues in making team and department decisions" ),
      new Task("Identifying and addressing my professional development needs" )
        ]),
    IncreaseItems: ko.observableArray(),
    LeaveItems: ko.observableArray(),
    ReduceItems: ko.observableArray(),
    selectedTask: ko.observable(),
    selectTask: function(task) {
        this.selectedTask(task);
    },

};

//connect items with observableArrays
ko.bindingHandlers.sortableList = {
    init: function(element, valueAccessor, allBindingsAccessor, context) {
        $(element).data("sortList", valueAccessor()); //attach meta-data
        $(element).sortable({
            update: function(event, ui) {
                var item = ui.item.data("sortItem");
                if (item) {
                    //identify parents
                    var originalParent = ui.item.data("parentList");
                    var newParent = ui.item.parent().data("sortList");
                    //figure out its new position
                    var position = ko.utils.arrayIndexOf(ui.item.parent().children(), ui.item[0]);
                    if (position >= 0) {
                        originalParent.remove(item);
                        newParent.splice(position, 0, item);
                    }

                    ui.item.remove();
                }
            },
            connectWith: '.container'
        });
    }
};

//attach meta-data
ko.bindingHandlers.sortableItem = {
    init: function(element, valueAccessor) {
        var options = valueAccessor();
        $(element).data("sortItem", options.item);
        $(element).data("parentList", options.parentList);
    }
};

//control visibility, give element focus, and select the contents (in order)
ko.bindingHandlers.visibleAndSelect = {
    update: function(element, valueAccessor) {
        ko.bindingHandlers.visible.update(element, valueAccessor);
        if (valueAccessor()) {
            setTimeout(function() {
                $(element).focus().select();
            }, 0); //new tasks are not in DOM yet
        }
    }
}

ko.applyBindings(viewModel);