sap.ui.define(
  [
    "sap/ui/core/UIComponent",
    "sap/ui/model/json/JSONModel",
    "sap/ui/core/routing/Router",
    "todo/model/models",
  ],
  function (UIComponent, JSONModel, Router, models) {
    "use strict";

    return UIComponent.extend("todo.Component", {
      metadata: {
        manifest: "json",
      },

      init: function () {
        UIComponent.prototype.init.apply(this, arguments);

        this.getRouter().initialize();

        var oData = {
          tasks: [],
          page: 1,
          pageSize: 10,
          totalPages: 1,
          busy: false,
        };
        var oModel = new JSONModel(oData);
        this.setModel(oModel, "tasks");

        this.setModel(models.createDeviceModel(), "device");
      },
    });
  }
);
