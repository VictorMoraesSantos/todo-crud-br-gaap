sap.ui.define(
  [
    "sap/ui/core/mvc/Controller",
    "sap/ui/model/json/JSONModel",
    "sap/m/MessageToast",
    "sap/ui/core/mvc/XMLView",
    "todo/config",
  ],
  function (Controller, JSONModel, MessageToast, XMLView, config) {
    "use strict";

    return Controller.extend("todo.controller.TaskList", {
      onInit: function () {
        this._loadTasks();
      },

      _loadTasks: function (page = 1, pageSize = 10, title) {
        var oModel = this.getView().getModel("tasks");
        if (!oModel) {
          var oComp = this.getOwnerComponent && this.getOwnerComponent();
          if (oComp && oComp.getModel) {
            oModel = oComp.getModel("tasks");
          }
        }
        if (!oModel) {
          oModel = new JSONModel({
            tasks: [],
            page: page,
            pageSize: pageSize,
            totalPages: 1,
            busy: false,
          });
          this.getView().setModel(oModel, "tasks");
        }
        oModel.setProperty("/busy", true);
        oModel.setProperty("/page", page);
        oModel.setProperty("/pageSize", pageSize);
        var url = `/api/todos?page=${page}&pageSize=${pageSize}`;
        if (title) {
          url += `&title=${encodeURIComponent(title)}`;
        }

        fetch(config.apiBase + url)
          .then(function (response) {
            if (!response.ok) {
              return response.text().then(function (text) {
                var err = new Error("Network response was not ok");
                err.status = response.status;
                err.body = text;
                throw err;
              });
            }
            return response.json();
          })
          .then(function (data) {
            try {
              console.log("_loadTasks response:", data);
            } catch (e) {}
            var items = [];
            if (Array.isArray(data)) {
              items = data;
            } else if (data && Array.isArray(data.items)) {
              items = data.items;
            } else if (data && Array.isArray(data.todos)) {
              items = data.todos;
            } else if (data && Array.isArray(data.data)) {
              items = data.data;
            } else if (data && Array.isArray(data.results)) {
              items = data.results;
            } else if (data && typeof data === "object") {
              for (var k in data) {
                if (Array.isArray(data[k])) {
                  items = data[k];
                  break;
                }
              }
            }

            oModel.setProperty("/tasks", items || []);
            if (data && data.totalPages) {
              oModel.setProperty("/totalPages", data.totalPages);
            }
            oModel.setProperty("/busy", false);
          })
          .catch(function (err) {
            MessageToast.show(
              "Erro ao carregar tarefas: " +
                (err && (err.status || err.message)
                  ? err.status || err.message
                  : "unknown")
            );
            var sample = [
              { userId: 1, id: 1, title: "sample task one", completed: false },
              { userId: 1, id: 2, title: "sample task two", completed: true },
              { userId: 2, id: 3, title: "another sample", completed: false },
            ];
            oModel.setProperty("/tasks", sample);
            oModel.setProperty("/busy", false);
          });
      },

      onSearch: function (oEvent) {
        var sQuery = oEvent.getParameter("newValue");
        var that = this;
        clearTimeout(this._searchTimeout);
        this._searchTimeout = setTimeout(function () {
          that._loadTasks(1, 10, sQuery);
        }, 300);
      },

      onToggleCompleted: function (oEvent) {
        var oItem = oEvent.getSource().getBindingContext("tasks");
        if (!oItem) {
          return;
        }
        var oData = oItem.getObject();
        var updated = Object.assign({}, oData, {
          completed: oEvent.getParameter("selected"),
        });

        fetch(config.apiBase + `/api/todos/${updated.id}`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            title: updated.title,
            completed: updated.completed,
            userId: updated.userId,
          }),
        })
          .then(function (res) {
            if (!res.ok) throw new Error("Update failed");
            return res.json();
          })
          .then(function (data) {
            var oModel = oItem.getModel();
            var aTasks = oModel.getProperty("/tasks") || [];
            var idx = aTasks.findIndex(function (t) {
              return String(t.id) === String(updated.id);
            });
            if (idx !== -1) {
              aTasks[idx] = data;
              oModel.setProperty("/tasks", aTasks);
            }
          })
          .catch(function (err) {
            var oModel = oItem.getModel();
            try {
              var sPath = oItem.getPath();
              if (sPath) {
                oModel.setProperty(sPath + "/completed", oData.completed);
              } else {
                oModel.refresh();
              }
            } catch (e) {
              try {
                oModel.refresh();
              } catch (e2) {}
            }

            if (updated && updated.completed === false) {
              MessageToast.show(
                "O máximo de tarefas incompletas por usuário é 5."
              );
            } else {
              try {
                console.error("onToggleCompleted error (mark complete):", err);
              } catch (e) {}
            }
          });
      },

      _getBindingContextFromControl: function (oControl, sModelName) {
        var oCur = oControl;
        while (oCur) {
          try {
            if (oCur.getBindingContext) {
              var oCtx = oCur.getBindingContext(sModelName);
              if (oCtx) {
                return oCtx;
              }
            }
          } catch (e) {}
          if (oCur.getParent) {
            oCur = oCur.getParent();
          } else {
            oCur = null;
          }
        }
        return null;
      },

      onPrevPage: function () {
        var oModel = this.getView().getModel("tasks");
        var page = oModel.getProperty("/page") || 1;
        var pageSize = oModel.getProperty("/pageSize") || 10;
        if (page > 1) {
          this._loadTasks(page - 1, pageSize);
        }
      },

      onNextPage: function () {
        var oModel = this.getView().getModel("tasks");
        var page = oModel.getProperty("/page") || 1;
        var pageSize = oModel.getProperty("/pageSize") || 10;
        var total = oModel.getProperty("/totalPages");
        var aTasks = oModel.getProperty("/tasks") || [];
        var hasMore = false;
        if (typeof total === "number") {
          hasMore = page < (total || 1);
        }
        if (!hasMore) {
          hasMore = aTasks.length >= pageSize;
        }

        if (hasMore) {
          this._loadTasks(page + 1, pageSize);
        }
      },

      onDetails: function (oEvent) {
        var that = this;
        var oSource = oEvent.getSource();
        var oContext =
          oSource.getBindingContext("tasks") ||
          this._getBindingContextFromControl(oSource, "tasks");
        if (!oContext) {
          MessageToast.show("No item selected");
          return;
        }
        var oItem = oContext.getObject();

        // set currentTask in model so the details view can bind to it
        var oModel = this.getView().getModel("tasks");
        oModel.setProperty("/currentTask", Object.assign({}, oItem));

        // Try router navigation first (preserves URL), fallback to programmatic view creation
        try {
          var oRouter = sap.ui.core.UIComponent.getRouterFor(this);
          if (oRouter) {
            oRouter.navTo("TaskDetails", { taskId: oItem.id });
            return;
          }
        } catch (e) {}

        // Fallback: create the TaskDetails view and navigate to it inside the App control
        XMLView.create({ viewName: "todo.view.TaskDetails" }).then(function (
          oView
        ) {
          var oApp = that.byId("app");
          try {
            // If the view is not already added, add and navigate to it
            if (!oApp.indexOfPage || oApp.indexOfPage(oView) === -1) {
              oApp.addPage(oView);
            }
            oApp.to(oView);
          } catch (e) {
            // fallback: place view directly
            oView.placeAt("content");
          }
        });
      },
    });
  }
);
