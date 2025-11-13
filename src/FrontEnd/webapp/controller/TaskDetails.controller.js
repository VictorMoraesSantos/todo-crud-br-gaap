sap.ui.define(
  [
    "sap/ui/core/mvc/Controller",
    "sap/ui/model/json/JSONModel",
    "sap/m/MessageToast",
    "todo/config",
  ],
  function (Controller, JSONModel, MessageToast, config) {
    "use strict";

    return Controller.extend("todo.controller.TaskDetails", {
      onInit: function () {
        var oRouter = sap.ui.core.UIComponent.getRouterFor(this);
        oRouter
          .getRoute("TaskDetails")
          .attachPatternMatched(this._onObjectMatched, this);
      },

      _onObjectMatched: function (oEvent) {
        var taskId = oEvent.getParameter("arguments").taskId;
        var oModel = this.getView().getModel("tasks");

        var aTasks = oModel.getProperty("/tasks") || [];
        var oFound = aTasks.find(function (t) {
          return String(t.id) === String(taskId);
        });

        if (oFound) {
          oModel.setProperty("/currentTask", oFound);
          return;
        }

        fetch(config.apiBase + `/api/todos/${taskId}`)
          .then(function (res) {
            if (!res.ok) {
              throw new Error("Network response was not ok");
            }
            return res.json();
          })
          .then(function (data) {
            oModel.setProperty("/currentTask", data);
          })
          .catch(function () {
            oModel.setProperty("/currentTask", {
              id: taskId,
              title: "(not available)",
              completed: false,
              userId: null,
            });
          });
      },

      onSave: function () {
        var oModel = this.getView().getModel("tasks");
        var oCurrent = oModel.getProperty("/currentTask");
        if (!oCurrent || !oCurrent.id) {
          MessageToast.show("Nenhuma tarefa para salvar");
          return;
        }

        var payload = {
          title: oCurrent.title,
          completed: oCurrent.completed,
          userId: oCurrent.userId,
        };

        fetch(config.apiBase + `/api/todos/${oCurrent.id}`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(payload),
        })
          .then(function (res) {
            if (!res.ok) throw new Error("Save failed");
            return res.text().then(function (txt) {
              try {
                return txt ? JSON.parse(txt) : null;
              } catch (e) {
                return txt;
              }
            });
          })
          .then(function (data) {
            var saved = data && typeof data === "object" ? data : oCurrent;
            var aTasks = oModel.getProperty("/tasks") || [];
            var aTasks = oModel.getProperty("/tasks") || [];
            var idx = aTasks.findIndex(function (t) {
              return String(t.id) === String(saved.id);
            });
            if (idx !== -1) {
              aTasks[idx] = saved;
              oModel.setProperty("/tasks", aTasks);
            }
            oModel.setProperty("/currentTask", saved);
            MessageToast.show("Tarefa salva com sucesso");
          })
          .catch(function (err) {
            try {
              console.error("onSave error:", err);
            } catch (e) {}
            MessageToast.show(
              "O máximo de tarefas incompletas por usuário é 5."
            );
          });
      },
    });
  }
);
