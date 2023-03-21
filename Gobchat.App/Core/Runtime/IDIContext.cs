/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

namespace Gobchat.Core.Runtime
{
    public interface IDIContext : System.IDisposable
    {
        RegisterType Resolve<RegisterType>() where RegisterType : class;

        RegisterType Resolve<RegisterType>(string name) where RegisterType : class;

        void Register<RegisterType>(System.Func<IDIContext, object, RegisterType> factory) where RegisterType : class;

        void Register<RegisterType>(System.Func<IDIContext, object, RegisterType> factory, string name) where RegisterType : class;

        void Register<RegisterType>(RegisterType instance) where RegisterType : class;

        void Register<RegisterType>(string name) where RegisterType : class;

        void Register<RegisterType>(RegisterType instance, string name) where RegisterType : class;

        void Register<RegisterType>() where RegisterType : class;

        void Unregister<RegisterType>(string name) where RegisterType : class;

        void Unregister<RegisterType>() where RegisterType : class;

        IDIContext GetParent();

        IDIContext CreateChild();

        IDIContext CreateChild(string name);
    }
}